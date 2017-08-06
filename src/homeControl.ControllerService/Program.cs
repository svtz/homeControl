using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using homeControl.Configuration.IoC;
using homeControl.Events.Bindings;
using homeControl.Events.Sensors;
using homeControl.Events.Switches;
using homeControl.Events.System;
using homeControl.Interop.Rabbit.IoC;
using RabbitMQ.Client;
using StructureMap;

namespace homeControl.ControllerService
{
    internal sealed class Program
    {
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
            //var initializers = workContainer.GetAllInstances<IInitializer>();
            //foreach (var initializer in initializers)
            //{
            //    initializer.Init();
            //}

            var bindingsProcessor = workContainer.GetInstance<BindingEventsProcessor>();
            var sensorsProcessor = workContainer.GetInstance<SensorEventsProcessor>();

            Task.WaitAll(
                bindingsProcessor.Run(ct),
                sensorsProcessor.Run(ct));
        }
    }
}