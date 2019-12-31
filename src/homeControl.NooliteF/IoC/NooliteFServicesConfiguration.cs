using homeControl.NooliteF.Adapters;
using homeControl.NooliteF.Configuration;
using homeControl.NooliteF.SwitchController;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace homeControl.NooliteF.IoC
{
    internal static class NooliteFServicesConfiguration
    {
        public static void AddNooliteFServices(this IServiceCollection services, IConfigurationRoot config)
        {
            services.AddSingleton<IMtrfAdapter>(
                sp => new AdapterWrapper(config["AdapterPort"], sp.GetRequiredService<ILogger>()));

            services.AddSingleton<ISwitchController, NooliteFSwitchController>();
            services.AddSingleton<INooliteFSwitchInfoRepository, NooliteFSwitchInfoRepository>();
            services.AddSingleton<INooliteFSensorInfoRepository, NooliteFSensorInfoRepository>();
            services.AddSingleton<NooliteFSensor>();
            services.AddSingleton<SwitchEventsProcessorF>();
            services.AddSingleton<SwitchEventsObserverF>();
            services.AddSingleton<NooliteFSwitchesStatusHolder>();
        }
    }
}