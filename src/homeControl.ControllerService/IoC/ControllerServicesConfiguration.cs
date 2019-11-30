using homeControl.ControllerService.Bindings;
using homeControl.ControllerService.Sensors;
using Microsoft.Extensions.DependencyInjection;

namespace homeControl.ControllerService.IoC
{
    internal static class ControllerServicesConfiguration
    {
        public static void AddControllerServices(this IServiceCollection services)
        {
            services.AddTransient<BindingController>();

            services.AddSingleton<IBindingController>(sp => sp.GetRequiredService<BindingController>());
            services.AddSingleton<IBindingStateManager>(sp => sp.GetRequiredService<BindingController>());

            services.AddTransient<BindingEventsProcessor>();
            services.AddTransient<SensorEventsProcessor>();
        }
    }
}