using System.Collections.Generic;
using System.Text;
using System.Threading;
using homeControl.Domain.Events.Configuration;
using homeControl.Entry;
using homeControl.Interop.Rabbit.IoC;
using RabbitMQ.Client;
using StructureMap;

namespace homeControl.ConfigurationStore
{
    internal sealed class ConfigurationStoreService : AbstractService
    {
        protected override string ServiceNamePrefix => "configStore";

        protected override IEnumerable<Registry> GetConfigurationRegistries(string uniqueServiceName)
        {
            yield return new RabbitConfigurationRegistryBuilder(Config)
                .UseJsonSerializationWithEncoding(Encoding.UTF8)
                .SetupEventSource<ConfigurationRequestEvent>("configuration-requests", ExchangeType.Fanout, string.Empty)
                .SetupEventSender<ConfigurationResponseEvent>("configuration")
                .Build();

            yield return new ConfigurationStoreRegistry();
        }

        protected override void Run(IContainer workContainer, CancellationToken ct)
        {
            workContainer.GetInstance<ConfigurationRequestsProcessor>().Run(ct).Wait(ct);
        }

        private static void Main(string[] args)
        {
            new ConfigurationStoreService().Run();
        }
    }
}