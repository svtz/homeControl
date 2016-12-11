using System.Collections.Generic;
using System.Linq;

namespace homeControl.Configuration.Switches
{
    internal sealed class SwitchConfgurationRepository : ISwitchConfigurationRepository
    {
        private readonly IDictionary<SwitchId, ISwitchConfiguration> _configurations;

        public SwitchConfgurationRepository(IDictionary<SwitchId, ISwitchConfiguration> configurations)
        {
            Guard.DebugAssertArgumentNotNull(configurations, nameof(configurations));
            if (configurations.Any(cfg => cfg.Value == null))
                throw new InvalidConfigurationException($"Found null-configuration for switch.");

            _configurations = configurations;
        }

        public bool ContainsConfig<TConfig>(SwitchId switchId) where TConfig : ISwitchConfiguration
        {
            ISwitchConfiguration config;
            return _configurations.TryGetValue(switchId, out config) && config is TConfig;
        }

        public TConfig GetConfig<TConfig>(SwitchId switchId) where TConfig : ISwitchConfiguration
        {
            return (TConfig)_configurations[switchId];
        }

        public SwitchId[] GetAllIds()
        {
            return _configurations.Keys.ToArray();
        }
    }
}