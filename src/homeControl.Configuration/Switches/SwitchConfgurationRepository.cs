using System;
using System.Collections.Generic;
using System.Linq;

namespace homeControl.Configuration.Switches
{
    internal sealed class SwitchConfgurationRepository : ISwitchConfigurationRepository
    {
        private readonly IDictionary<SwitchId, ISwitchConfiguration> _configurations;

        public SwitchConfgurationRepository(ISwitchConfiguration[] configurations)
        {
            Guard.DebugAssertArgumentNotNull(configurations, nameof(configurations));
            if (configurations.Any(cfg => cfg == null))
                throw new InvalidConfigurationException($"Found null-configuration for switch.");

            try
            {
                _configurations = configurations.ToDictionary(cfg => cfg.SwitchId);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidConfigurationException(ex, "Found duplicated switch ids in the configuration file.");
            }
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

        public SwitchId[] GetAll()
        {
            return _configurations.Keys.ToArray();
        }
    }
}