using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using homeControl.Configuration.IoC;
using homeControl.ControllerService.Bindings;
using homeControl.ControllerService.IoC;
using homeControl.ControllerService.Sensors;
using homeControl.Domain.Events.Bindings;
using homeControl.Domain.Events.Configuration;
using homeControl.Domain.Events.Sensors;
using homeControl.Domain.Events.Switches;
using homeControl.Entry;
using homeControl.Interop.Rabbit.IoC;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace homeControl.ControllerService
{
    internal sealed class ControllerService : AbstractService
    {
        protected override string ServiceNamePrefix => "controller";
        protected override async Task Run(IServiceProvider serviceProvider, CancellationToken ct)
        {
            var bindingsProcessor = serviceProvider.GetRequiredService<BindingEventsProcessor>();
            var sensorsProcessor = serviceProvider.GetRequiredService<SensorEventsProcessor>();

            await Task.WhenAll(
                bindingsProcessor.Run(ct),
                sensorsProcessor.Run(ct));
        }

        protected override void ConfigureServices(ServiceCollection services, string uniqueServiceName)
        {
            new RabbitConfiguration(Config)
                .UseJsonSerializationWithEncoding(Encoding.UTF8)
                .SetupEventSender<ConfigurationRequestEvent>("configuration-requests")
                .SetupEventSource<ConfigurationResponseEvent>("configuration", ExchangeType.Direct, uniqueServiceName)
                .SetupEventSource<AbstractBindingEvent>("main", ExchangeType.Fanout, "")
                .SetupEventSource<AbstractSensorEvent>("main", ExchangeType.Fanout, "")
                .SetupEventSender<AbstractSwitchEvent>("main")
                .Apply(services);
            
            services.AddConfigurationRepositories(uniqueServiceName);
            services.AddControllerServices();
        }

        private static async Task Main(string[] args)
        {
            await new ControllerService().Run();
        }
    }
}