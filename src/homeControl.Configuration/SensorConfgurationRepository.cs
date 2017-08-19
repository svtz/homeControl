using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using homeControl.Domain;
using homeControl.Domain.Repositories;
using JetBrains.Annotations;
using Serilog;

namespace homeControl.Configuration
{
    [UsedImplicitly]
    internal sealed class SensorConfgurationRepository : 
        AbstractConfigurationRepository<SensorConfiguration[], SensorConfiguration[]>,
        ISensorConfigurationRepository
    {
        public SensorConfgurationRepository(IConfigurationLoader<SensorConfiguration[]> configLoader, ILogger logger)
            : base("sensors", configLoader, PrepareConfiguration, logger)
        {
        }

        private static SensorConfiguration[] PrepareConfiguration(SensorConfiguration[] configurations)
        {
            Guard.DebugAssertArgumentNotNull(configurations, nameof(configurations));
            
            if (configurations.Any(cfg => cfg == null))
                throw new InvalidConfigurationException("Found null-configuration for sensor.");
            if (configurations.Any(cfg => cfg.SensorId?.Id == Guid.Empty))
                throw new InvalidConfigurationException("Found zero identifier in the sensor config.");

            return configurations;
        }

        public async Task<IReadOnlyCollection<SensorConfiguration>> GetAll()
        {
            return await GetConfiguration();
        }
    }
}