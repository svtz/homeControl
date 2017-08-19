using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using homeControl.NooliteService.Configuration;
using JetBrains.Annotations;
using Serilog;

namespace homeControl.Configuration
{
    [UsedImplicitly]
    internal sealed class NooliteSensorInfoRepository : 
        AbstractConfigurationRepository<NooliteSensorInfo[], NooliteSensorInfo[]>, INooliteSensorInfoRepository

    {
        public NooliteSensorInfoRepository(IConfigurationLoader<NooliteSensorInfo[]> configLoader, ILogger logger)
            : base("sensors-noolite", configLoader, PrepareConfiguration, logger)
        {
        }

        private static NooliteSensorInfo[] PrepareConfiguration(NooliteSensorInfo[] configurations)
        {
            Guard.DebugAssertArgumentNotNull(configurations, nameof(configurations));
            
            if (configurations.Any(cfg => cfg == null))
                throw new InvalidConfigurationException("Found null-configuration for sensor.");
            if (configurations.Any(cfg => cfg.SensorId?.Id == Guid.Empty))
                throw new InvalidConfigurationException("Found zero identifier in the sensor config.");

            return configurations;
        }

        public async Task<IReadOnlyCollection<NooliteSensorInfo>> GetAll()
        {
            return await GetConfiguration();
        }
    }
}