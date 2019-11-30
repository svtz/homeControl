using homeControl.Configuration;
using homeControl.NooliteService.Adapters;
using homeControl.NooliteService.Configuration;
using homeControl.NooliteService.SwitchController;
using Microsoft.Extensions.DependencyInjection;

namespace homeControl.NooliteService.IoC
{
    internal static class NooliteServicesConfiguration
    {
        public static void AddNooliteServices(this IServiceCollection services)
        {
            services.AddSingleton<IPC11XXAdapter, PC11XXAdapterWrapper>();
            services.AddSingleton<IRX2164Adapter, RX2164AdapterWrapper>();
            
            services.AddSingleton<ISwitchController, NooliteSwitchController>();
            services.AddSingleton<INooliteSwitchInfoRepository, NooliteSwitchInfoRepository>();
            services.AddSingleton<INooliteSensorInfoRepository, NooliteSensorInfoRepository>();

            services.AddSingleton<NooliteSensor>();
        }
    }
}