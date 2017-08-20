using System;
using System.Reflection;
using System.Text;
using System.Threading;
using homeControl.Configuration;
using homeControl.Configuration.IoC;
using homeControl.Domain.Events.Configuration;
using homeControl.Domain.Events.Sensors;
using homeControl.Domain.Events.Switches;
using homeControl.Interop.Rabbit.IoC;
using homeControl.NooliteService.Adapters;
using homeControl.NooliteService.Configuration;
using homeControl.NooliteService.SwitchController;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Serilog;
using Serilog.Events;
using StructureMap;

namespace homeControl.NooliteService
{
    class NooliteServiceEntry
    {
        private static readonly ILogger _log;
        private static readonly IConfigurationRoot _config =
            new ConfigurationBuilder()
                .AddJsonFile("settings.json")
                .Build();

        static NooliteServiceEntry()
        {
            var level = (LogEventLevel)Enum.Parse(typeof(LogEventLevel), _config["LogEventLevel"]);

            _log = new LoggerConfiguration()
                .MinimumLevel.Is(level)
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} (from {SourceContext}){NewLine}{Exception}")
                .CreateLogger()
                .ForContext(typeof(NooliteServiceEntry));
            _log.Debug("Logging initialized.");
        }

        private static readonly IContainer _rootContainer = BuildContainer();
        private static IContainer BuildContainer()
        {
            var serviceName = "noolite-" + Guid.NewGuid();

            var container = new Container(cfg =>
            {
                cfg.AddRegistry(new RabbitConfigurationRegistryBuilder(_config)
                    .UseJsonSerializationWithEncoding(Encoding.UTF8)
                    .SetupEventSender<ConfigurationRequestEvent>("configuration-requests")
                    .SetupEventSource<ConfigurationResponseEvent>("configuration", ExchangeType.Direct, serviceName)
                    .SetupEventSender<AbstractSensorEvent>("main")
                    .SetupEventSource<AbstractSwitchEvent>("main", ExchangeType.Fanout, "")
                    .Build());

                cfg.AddRegistry(new ConfigurationRegistry(serviceName));

                cfg.For<IPC11XXAdapter>().Use<PC11XXAdapterWrapper>().Singleton();
                cfg.For<IRX2164Adapter>().Use<RX2164AdapterWrapper>().Singleton();
                //cfg.For<ISwitchController>().Add<NooliteSwitchController>().Singleton();
                cfg.For<ISwitchController>().Add<SwitchControllerConsoleEmulator>().Singleton();
                cfg.For<INooliteSwitchInfoRepository>().Use<NooliteSwitchInfoRepository>().Transient();
                cfg.For<INooliteSensorInfoRepository>().Use<NooliteSensorInfoRepository>().Transient();
                cfg.ForConcreteType<NooliteSensor>().Configure.Transient();

                cfg.For<ILogger>().Use(c => _log.ForContext(c.ParentType));
            });

            return container;
        }

        static void Main(string[] args)
        {
            var asmName = Assembly.GetEntryAssembly().GetName();
            Console.Title = $"{asmName.Name} v.{asmName.Version.ToString(3)}";

            _log.Information($"Starting service: {Console.Title}");

            using (var workContainer = _rootContainer.GetNestedContainer())
            using (var cts = new CancellationTokenSource())
            {
                workContainer.Configure(c => c.For<CancellationToken>().Use(() => cts.Token));

                Console.CancelKeyPress += (s, e) => cts.Cancel();

                Run(workContainer, cts.Token);
            }
        }

        private static void Run(IContainer workContainer, CancellationToken ct)
        {
            workContainer.GetInstance<NooliteSensor>().Activate();
            
            var switchesProcessor = workContainer.GetInstance<SwitchEventsProcessor>();

            switchesProcessor.Run(ct).Wait(ct);
        }
    }
}