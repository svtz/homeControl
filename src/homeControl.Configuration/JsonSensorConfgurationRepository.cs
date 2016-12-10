using System.Linq;

namespace homeControl.Configuration
{
    internal sealed class SensorConfgurationRepository : ISensorConfigurationRepository
    {
        private readonly ISensorConfiguration[] _configurations;

        public SensorConfgurationRepository(ISensorConfiguration[] configurations)
        {
            Guard.DebugAssertArgumentNotNull(configurations, nameof(configurations));
            if (configurations.Any(cfg => cfg == null))
                throw new InvalidConfigurationException("Found null-configuration for sensor.");

            _configurations = configurations;
        }

        public TSensorConfig[] GetAllConfigs<TSensorConfig>() where TSensorConfig : ISensorConfiguration
        {
            return _configurations.OfType<TSensorConfig>().ToArray();
        }
    }
}