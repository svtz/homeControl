using System;
using homeControl.Configuration.Sensors;
using homeControl.Configuration.Switches;
using StructureMap;

namespace homeControl.Configuration.IoC
{
    public sealed class ConfigurationRegistry : Registry
    {
        public ConfigurationRegistry(string configPath)
        {
            var config = JsonConfigurationStore.Load(configPath);

            For<ISensorConfigurationRepository>().Use(new SensorConfgurationRepository(config.SensorConfigurations));
            For<ISwitchConfigurationRepository>().Use(new SwitchConfgurationRepository(config.SwitchConfigurations));
        }
    }
}
