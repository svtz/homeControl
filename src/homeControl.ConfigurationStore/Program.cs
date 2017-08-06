using System.Text;
using homeControl.Events.System;
using homeControl.Interop.Rabbit.IoC;
using RabbitMQ.Client;
using StructureMap;

namespace homeControl.ConfigurationStore
{
    internal sealed class Program
    {
        private static readonly IContainer _rootContainer = BuildContainer();
        private static IContainer BuildContainer()
        {
            var container = new Container(cfg =>
            {
                cfg.ForSingletonOf<ConfigurationProvider>();
                cfg.ForConcreteType<ConfigurationRequestsProcessor>();

                cfg.AddRegistry(new RabbitConfigurationRegistryBuilder("amqp://configStore:configStore@192.168.1.17/debug")
                    .UseJsonSerializationWithEncoding(Encoding.UTF8)
                    .SetupEventSource<ConfigurationRequestEvent>("configuration_requests", ExchangeType.Fanout, string.Empty)
                    .SetupEventSender<ConfigurationResponseEvent>("configuration", ExchangeType.Direct)
                    .Build());
            });

            return container;
        }

        static void Main(string[] args)
        {
            using (var workContainer = _rootContainer.GetNestedContainer())
            {
                workContainer.GetInstance<ConfigurationRequestsProcessor>().Run();
            }
        }
    }
}