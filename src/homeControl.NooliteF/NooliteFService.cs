using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using homeControl.Configuration.IoC;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Configuration;
using homeControl.Domain.Events.Sensors;
using homeControl.Domain.Events.Switches;
using homeControl.Entry;
using homeControl.Interop.Rabbit.IoC;
using homeControl.NooliteF.IoC;
using homeControl.NooliteF.SwitchController;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace homeControl.NooliteF
{
    public class NooliteFService : AbstractService
    {
        protected override string ServiceNamePrefix { get; } = "noolite-f";
        protected override async Task Run(IServiceProvider serviceProvider, CancellationToken ct)
        {
            serviceProvider.GetRequiredService<NooliteFSensor>().Activate();
            await serviceProvider.GetRequiredService<ISwitchController>().InitializeState();
            
            var switchesProcessor = serviceProvider.GetRequiredService<SwitchEventsProcessorF>();

            switchesProcessor.Start(ct);

            var statusReporter = serviceProvider.GetRequiredService<StatusReporter>();
            await Task.WhenAll(
                switchesProcessor.Completion(ct),
                statusReporter.Run(ct)
            );
        }

        protected override void ConfigureServices(ServiceCollection services, string uniqueServiceName)
        {
            new RabbitConfiguration(Config)
                .UseJsonSerializationWithEncoding(Encoding.UTF8)
                .SetupEventSender<ConfigurationRequestEvent>("configuration-requests")
                .SetupEventSource<ConfigurationResponseEvent>("configuration", ExchangeType.Direct, uniqueServiceName)
                .SetupEventSender<AbstractSensorEvent>("main")
                .SetupEventSender<AbstractSwitchEvent>("main")
                .SetupEventSource<NeedStatusEvent>("main", ExchangeType.Fanout, "")
                .SetupEventSource<AbstractSwitchEvent>("main", ExchangeType.Fanout, "")
                .Apply(services);
            
            services.AddConfigurationRepositories(uniqueServiceName);
            services.AddNooliteFServices(Config);
        }

        private static async Task Main(string[] args)
        {
            await new NooliteFService().Run();
        }
    }
}