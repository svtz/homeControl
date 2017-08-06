using System;
using System.Reflection;
using System.Text;
using System.Threading;
using homeControl.Application;
using homeControl.Configuration.IoC;
using homeControl.Events.Sensors;
using homeControl.Events.Switches;
using homeControl.Events.System;
using homeControl.Interop.Rabbit.IoC;
using homeControl.Noolite.Adapters;
using homeControl.Peripherals;
using RabbitMQ.Client;
using Serilog;
using Serilog.Events;
using StructureMap;

namespace homeControl.NooliteService
{
    class Program
    {
        private static readonly ILogger _log;
        static Program()
        {
#if DEBUG
            var level = LogEventLevel.Verbose;
#else
            var level = LogEventLevel.Information;
#endif
            _log = new LoggerConfiguration()
                .MinimumLevel.Is(level)
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} (from {SourceContext}){NewLine}{Exception}")
                .CreateLogger()
                .ForContext(typeof(Program));
            _log.Debug("Logging initialized.");
        }

        private static readonly IContainer _rootContainer = BuildContainer();
        private static IContainer BuildContainer()
        {
            var serviceName = "noolite-" + Guid.NewGuid();

            var container = new Container(cfg =>
            {
                cfg.AddRegistry(new RabbitConfigurationRegistryBuilder("amqp://noolite:noolite@192.168.1.17/debug")
                    .UseJsonSerializationWithEncoding(Encoding.UTF8)
                    .SetupEventSender<ConfigurationRequestEvent>("configuration_requests", ExchangeType.Fanout)
                    .SetupEventSource<ConfigurationResponseEvent>("configuration", ExchangeType.Direct, serviceName)
                    .SetupEventSender<AbstractSensorEvent>("main", ExchangeType.Fanout)
                    .SetupEventSource<AbstractSwitchEvent>("main", ExchangeType.Fanout, "")
                    .Build());

                cfg.AddRegistry(new ConfigurationRegistry(serviceName));

                cfg.For<IPC11XXAdapter>().Use<PC11XXAdapterWrapper>().Singleton();
                cfg.For<IRX2164Adapter>().Use<RX2164AdapterWrapper>().Singleton();
                //cfg.For<ISwitchController>().Add<NooliteSwitchController>().Singleton();
                cfg.For<ISwitchController>().Add<SwitchControllerConsoleEmulator>().Singleton();
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