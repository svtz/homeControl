using System;
using System.Collections.Generic;
using System.Linq;

namespace homeControl.Configuration.Sensors
{
    internal sealed class SensorConfgurationRepository : 
        AbstractConfigurationRepository<ISensorConfiguration[], ISensorConfiguration[]>,
        ISensorConfigurationRepository
    {
        public SensorConfgurationRepository(IConfigurationLoader<ISensorConfiguration[]> configLoader)
            : base("sensors", configLoader, PrepareConfiguration)
        {
        }

        private static ISensorConfiguration[] PrepareConfiguration(ISensorConfiguration[] configurations)
        {
            Guard.DebugAssertArgumentNotNull(configurations, nameof(configurations));
            
            if (configurations.Any(cfg => cfg == null))
                throw new InvalidConfigurationException("Found null-configuration for sensor.");
            if (configurations.Any(cfg => cfg.SensorId?.Id == Guid.Empty))
                throw new InvalidConfigurationException("Found zero identifier in the sensor config.");

            return configurations;
        }

        public IReadOnlyCollection<TSensorConfig> GetAll<TSensorConfig>() where TSensorConfig : ISensorConfiguration
        {
            return Configuration.OfType<TSensorConfig>().ToArray();
        }
    }
}