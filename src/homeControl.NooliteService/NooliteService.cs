using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using homeControl.Configuration.IoC;
using homeControl.Domain.Events.Configuration;
using homeControl.Domain.Events.Sensors;
using homeControl.Domain.Events.Switches;
using homeControl.Entry;
using homeControl.Interop.Rabbit.IoC;
using homeControl.NooliteService.IoC;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace homeControl.NooliteService
{
    public class NooliteService : AbstractService
    {
        protected override string ServiceNamePrefix { get; } = "noolite";
        protected override async Task Run(IServiceProvider serviceProvider, CancellationToken ct)
        {
            Logger.Debug("Run: the beginning");
            
            serviceProvider.GetRequiredService<NooliteSensor>().Activate();
            
            var switchesProcessor = serviceProvider.GetRequiredService<SwitchEventsProcessor>();
 
            Logger.Debug("Starting SwitchEventsProcessor");
            switchesProcessor.Start(ct);
            await switchesProcessor.Completion(ct);
        }

        protected override void ConfigureServices(ServiceCollection services, string uniqueServiceName)
        {
            new RabbitConfiguration(Config)
                .UseJsonSerializationWithEncoding(Encoding.UTF8)
                .SetupEventSender<ConfigurationRequestEvent>("configuration-requests")
                .SetupEventSource<ConfigurationResponseEvent>("configuration", ExchangeType.Direct, uniqueServiceName)
                .SetupEventSender<AbstractSensorEvent>("main")
                .SetupEventSource<AbstractSwitchEvent>("main", ExchangeType.Fanout, "")
                .Apply(services);
            
            services.AddConfigurationRepositories(uniqueServiceName);
            services.AddNooliteServices();
        }
        
        private static void Main(string[] args)
        {
            new NooliteService().Run();
        }
    }
}