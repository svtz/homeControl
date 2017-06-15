using System;
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
using StructureMap;

namespace homeControl.NooliteService
{
    class Program
    {
        private static readonly IContainer _rootContainer = BuildContainer();
        private static IContainer BuildContainer()
        {
            var serviceName = "noolite-" + Guid.NewGuid();

            var container = new Container(cfg =>
            {
                cfg.AddRegistry(new RabbitConfigurationRegistryBuilder("")
                    .UseJsonSerialization()
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
            });

            return container;
        }

        static void Main(string[] args)
        {
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