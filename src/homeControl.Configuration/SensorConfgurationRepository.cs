using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using homeControl.Domain;
using homeControl.Domain.Repositories;
using JetBrains.Annotations;

namespace homeControl.Configuration
{
    [UsedImplicitly]
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

        public async Task<IReadOnlyCollection<TSensorConfig>> GetAll<TSensorConfig>() where TSensorConfig : ISensorConfiguration
        {
            return (await GetConfiguration()).OfType<TSensorConfig>().ToArray();
        }
    }
}