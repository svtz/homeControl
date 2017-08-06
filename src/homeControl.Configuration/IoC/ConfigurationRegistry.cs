using System;
using homeControl.Configuration.Bindings;
using homeControl.Configuration.Sensors;
using homeControl.Configuration.Switches;
using Newtonsoft.Json;
using StructureMap;

namespace homeControl.Configuration.IoC
{
    public sealed class ConfigurationRegistry : Registry
    {
        public ConfigurationRegistry(string serviceName)
        {
            Guard.DebugAssertArgumentNotNull(serviceName, nameof(serviceName));

            For<ISensorConfigurationRepository>().Use<SensorConfgurationRepository>().Transient();
            For<ISwitchConfigurationRepository>().Use<SwitchConfgurationRepository>().Transient();
            For<ISwitchToSensorBindingsRepository>().Use<SwitchToSensorBindingsRepository>().Transient();

            For<JsonConverter>().Add<SwitchIdSerializer>().Singleton();
            For<JsonConverter>().Add<SensorIdSerializer>().Singleton();

            For(typeof(IConfigurationLoader<>))
                .Use(typeof(JsonConfigurationLoader<>))
                .Ctor<string>("serviceName").Is(serviceName)
                .Transient();
        }
    }
}
