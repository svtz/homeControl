using System;
using System.Text;
using System.Threading;
using homeControl.ConfigurationStore.IoC;
using homeControl.Domain.Events.Configuration;
using homeControl.Entry;
using homeControl.Interop.Rabbit.IoC;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace homeControl.ConfigurationStore
{
    internal sealed class ConfigurationStoreService : AbstractService
    {
        protected override string ServiceNamePrefix => "configStore";
        protected override void Run(IServiceProvider serviceProvider, CancellationToken ct)
        {
            serviceProvider.GetRequiredService<ConfigurationRequestsProcessor>().Run(ct).Wait(ct);
        }

        protected override void ConfigureServices(ServiceCollection services, string uniqueServiceName)
        {
            new RabbitConfiguration(Config)
                .UseJsonSerializationWithEncoding(Encoding.UTF8)
                .SetupEventSource<ConfigurationRequestEvent>("configuration-requests", ExchangeType.Fanout, string.Empty)
                .SetupEventSender<ConfigurationResponseEvent>("configuration")
                .Apply(services);
            
            services.AddConfigurationStoreServices();
        }

        private static void Main(string[] args)
        {
            new ConfigurationStoreService().Run();
        }
    }
}