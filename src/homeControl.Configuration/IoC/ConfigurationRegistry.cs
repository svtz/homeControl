using homeControl.Configuration.Bindings;
using homeControl.Configuration.Sensors;
using homeControl.Configuration.Switches;
using StructureMap;

namespace homeControl.Configuration.IoC
{
    public sealed class ConfigurationRegistry : Registry
    {
        public ConfigurationRegistry(string configFolderPath)
        {
            For(typeof(IConfigurationLoader<>)).Use(typeof(JsonConfigurationLoader<>)).Singleton()
                                               .Ctor<string>().Is(configFolderPath);
            For<ISensorConfigurationRepository>().Use<SensorConfgurationRepository>();
            For<ISwitchConfigurationRepository>().Use<SwitchConfgurationRepository>();
            For<ISwitchToSensorBindingsRepository>().Use<SwitchToSensorBindingsRepository>();
        }
    }
}
