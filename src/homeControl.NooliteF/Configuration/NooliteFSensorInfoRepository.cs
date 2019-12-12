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
        AbstractConfigurationRepository<AbstractNooliteFSensorInfo[], AbstractNooliteFSensorInfo[]>, INooliteFSensorInfoRepository

    {
        public NooliteFSensorInfoRepository(IConfigurationLoader<AbstractNooliteFSensorInfo[]> configLoader, ILogger logger)
            : base("sensors-noolite-f", configLoader, PrepareConfiguration, logger)
        {
        }

        private static AbstractNooliteFSensorInfo[] PrepareConfiguration(AbstractNooliteFSensorInfo[] configurations)
        {
            Guard.DebugAssertArgumentNotNull(configurations, nameof(configurations));
            
            if (configurations.Any(cfg => cfg == null))
                throw new InvalidConfigurationException("Found null-configuration for sensor.");

            return configurations;
        }

        public async Task<IReadOnlyCollection<AbstractNooliteFSensorInfo>> GetAll()
        {
            return await GetConfiguration();
        }
    }
}