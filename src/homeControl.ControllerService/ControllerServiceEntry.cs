using System;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using homeControl.Configuration.IoC;
using homeControl.ControllerService.Bindings;
using homeControl.ControllerService.Sensors;
using homeControl.Domain.Events.Bindings;
using homeControl.Domain.Events.Configuration;
using homeControl.Domain.Events.Sensors;
using homeControl.Domain.Events.Switches;
using homeControl.Interop.Rabbit.IoC;
using RabbitMQ.Client;
using Serilog;
using Serilog.Events;
using StructureMap;

namespace homeControl.ControllerService
{
    internal sealed class ControllerServiceEntry
    {
        private static readonly ILogger _log;
        static ControllerServiceEntry()
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
                .ForContext(typeof(ControllerServiceEntry));
            _log.Debug("Logging initialized.");
        }

        private static readonly IContainer _rootContainer = BuildContainer();
        private static IContainer BuildContainer()
        {
            var serviceName = "controller-" + Guid.NewGuid();

            var container = new Container(cfg =>
            {
                cfg.AddRegistry(new RabbitConfigurationRegistryBuilder("amqp://controller:controller@192.168.1.17/debug")
                    .UseJsonSerializationWithEncoding(Encoding.UTF8)
                    .SetupEventSender<ConfigurationRequestEvent>("configuration_requests", ExchangeType.Fanout)
                    .SetupEventSource<ConfigurationResponseEvent>("configuration", ExchangeType.Direct, serviceName)
                    .SetupEventSource<AbstractBindingEvent>("main", ExchangeType.Fanout, "")
                    .SetupEventSource<AbstractSensorEvent>("main", ExchangeType.Fanout, "")
                    .SetupEventSender<AbstractSwitchEvent>("main", ExchangeType.Fanout)
                    .Build());

                cfg.AddRegistry(new ConfigurationRegistry(serviceName));

                cfg.ForConcreteType<BindingController>().Configure.Transient();

                cfg.Forward<BindingController, IBindingController>();
                cfg.Forward<BindingController, IBindingStateManager>();

                cfg.ForConcreteType<BindingEventsProcessor>().Configure.Transient();
                cfg.ForConcreteType<SensorEventsProcessor>().Configure.Transient();

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
            var bindingsProcessor = workContainer.GetInstance<BindingEventsProcessor>();
            var sensorsProcessor = workContainer.GetInstance<SensorEventsProcessor>();

            Task.WaitAll(
                bindingsProcessor.Run(ct),
                sensorsProcessor.Run(ct));
        }
    }
}