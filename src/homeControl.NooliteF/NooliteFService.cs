using System;
using System.Text;
using System.Threading;
using homeControl.Configuration.IoC;
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
        protected override void Run(IServiceProvider serviceProvider, CancellationToken ct)
        {
            serviceProvider.GetRequiredService<NooliteFSensor>().Activate();
            serviceProvider.GetRequiredService<ISwitchController>().InitializeState().Wait(ct);
            
            var switchesProcessor = serviceProvider.GetRequiredService<SwitchEventsProcessorF>();

            switchesProcessor.RunAsync(ct);
            switchesProcessor.Completion(ct).Wait(ct);
        }

        protected override void ConfigureServices(ServiceCollection services, string uniqueServiceName)
        {
            new RabbitConfiguration(Config)
                .UseJsonSerializationWithEncoding(Encoding.UTF8)
                .SetupEventSender<ConfigurationRequestEvent>("configuration-requests")
                .SetupEventSource<ConfigurationResponseEvent>("configuration", ExchangeType.Direct, uniqueServiceName)
                .SetupEventSender<AbstractSensorEvent>("main")
                .SetupEventSender<AbstractSwitchEvent>("main")
                .SetupEventSource<AbstractSwitchEvent>("main", ExchangeType.Fanout, "")
                .Apply(services);
            
            services.AddConfigurationRepositories(uniqueServiceName);
            services.AddNooliteFServices(Config);
        }

        private static void Main(string[] args)
        {
            new NooliteFService().Run();
        }
    }
}