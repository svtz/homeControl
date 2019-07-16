using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using homeControl.Configuration;
using JetBrains.Annotations;
using Serilog;

namespace homeControl.NooliteF.Configuration
{
    [UsedImplicitly]
    internal sealed class NooliteFSensorInfoRepository : 
        AbstractConfigurationRepository<NooliteFSensorInfo[], NooliteFSensorInfo[]>, INooliteFSensorInfoRepository

    {
        public NooliteFSensorInfoRepository(IConfigurationLoader<NooliteFSensorInfo[]> configLoader, ILogger logger)
            : base("sensors-noolite-f", configLoader, PrepareConfiguration, logger)
        {
        }

        private static NooliteFSensorInfo[] PrepareConfiguration(NooliteFSensorInfo[] configurations)
        {
            Guard.DebugAssertArgumentNotNull(configurations, nameof(configurations));
            
            if (configurations.Any(cfg => cfg == null))
                throw new InvalidConfigurationException("Found null-configuration for sensor.");
            if (configurations.Any(cfg => cfg.SensorId?.Id == Guid.Empty))
                throw new InvalidConfigurationException("Found zero identifier in the sensor config.");

            return configurations;
        }

        public async Task<IReadOnlyCollection<NooliteFSensorInfo>> GetAll()
        {
            return await GetConfiguration();
        }
    }
}