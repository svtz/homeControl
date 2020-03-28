using System;
using System.Threading;
using System.Threading.Tasks;
using homeControl.Configuration.IoC;
using homeControl.ControllerService.Bindings;
using homeControl.ControllerService.IoC;
using homeControl.ControllerService.Sensors;
using homeControl.Entry;
using Microsoft.Extensions.DependencyInjection;

namespace homeControl.ControllerService
{
    internal sealed class ControllerService : AbstractService
    {
        protected override string ServiceName => "Controller";
        protected override Task Run(IServiceProvider serviceProvider, CancellationToken ct)
        {
            var bindingsProcessor = serviceProvider.GetRequiredService<BindingEventsProcessor>();
            var sensorsProcessor = serviceProvider.GetRequiredService<SensorEventsProcessor>();

            return Task.WhenAll(
                bindingsProcessor.Run(ct),
                sensorsProcessor.Run(ct));
        }

        protected override void ConfigureServices(ServiceCollection services)
        {
            services.AddConfigurationRepositories(ServiceName);
            services.AddControllerServices();
        }

        private static async Task Main(string[] args)
        {
            await new ControllerService().Run();
        }
    }
}