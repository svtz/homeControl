using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace homeControl.ConfigurationStore.IoC
{
    internal static class ConfigurationStoreServicesConfiguration
    {
        public static void AddConfigurationStoreServices(this IServiceCollection services)
        {
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "conf");
            services.AddSingleton(sp => new ConfigurationProvider(configPath));
            services.AddSingleton<ConfigurationRequestsProcessor>();
        }
    }
}