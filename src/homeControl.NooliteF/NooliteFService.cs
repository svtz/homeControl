using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using homeControl.Configuration.IoC;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Configuration;
using homeControl.Domain.Events.Switches;
using homeControl.Entry;
using homeControl.NooliteF.IoC;
using homeControl.NooliteF.SwitchController;
using Microsoft.Extensions.DependencyInjection;

namespace homeControl.NooliteF
{
    public class NooliteFService : AbstractService
    {
        protected override string ServiceName { get; } = "Noolite-f";
        protected override Task Run(IServiceProvider serviceProvider, CancellationToken ct)
        {
            serviceProvider.GetRequiredService<NooliteFSensor>().Activate();
            serviceProvider.GetRequiredService<ISwitchController>().InitializeState().Wait(ct);
            
            var switchesProcessor = serviceProvider.GetRequiredService<SwitchEventsProcessorF>();

            switchesProcessor.RunAsync(ct);

            var statusReporter = serviceProvider.GetRequiredService<StatusReporter>();
            return Task.WhenAll(
                switchesProcessor.Completion(ct),
                statusReporter.Run(ct)
            );
        }

        protected override void ConfigureServices(ServiceCollection services)
        {
            services.AddConfigurationRepositories(ServiceName);
            services.AddNooliteFServices(Config);
        }

        private static async Task Main(string[] args)
        {
            await new NooliteFService().Run();
        }
    }
}