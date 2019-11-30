using homeControl.Configuration.Serializers;
using homeControl.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace homeControl.Configuration.IoC
{
    public static class ServiceCollectionConfiguration
    {
        public static void AddConfigurationRepositories(this IServiceCollection services, string serviceName)
        {
            Guard.DebugAssertArgumentNotNull(serviceName, nameof(serviceName));

            services.AddTransient<ISensorConfigurationRepository, SensorConfgurationRepository>();
            services.AddTransient<ISwitchConfigurationRepository, SwitchConfgurationRepository>();
            services.AddTransient<ISwitchToSensorBindingsRepository, SwitchToSensorBindingsRepository>();

            services.AddSingleton<JsonConverter, SwitchIdSerializer>();
            services.AddSingleton<JsonConverter, SensorIdSerializer>();
            services.AddSingleton<JsonConverter, StringEnumConverter>();

            services.AddSingleton(sp => new ServiceNameHolder(serviceName));
            services.AddTransient(typeof(IConfigurationLoader<>), typeof(JsonConfigurationLoader<>));
        }
    }
}
