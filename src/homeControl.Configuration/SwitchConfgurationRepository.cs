using System;
using System.Collections.Generic;
using System.Linq;

namespace homeControl.Configuration
{
    internal sealed class SwitchConfgurationRepository : ISwitchConfigurationRepository
    {
        private readonly IDictionary<Guid, ISwitchConfiguration> _configurations;

        public SwitchConfgurationRepository(IDictionary<Guid, ISwitchConfiguration> configurations)
        {
            Guard.DebugAssertArgumentNotNull(configurations, nameof(configurations));
            if (configurations.Any(cfg => cfg.Value == null))
                throw new InvalidConfigurationException($"Found null-configuration for switch.");

            _configurations = configurations;
        }

        public bool ContainsConfig<TConfig>(Guid switchId) where TConfig : ISwitchConfiguration
        {
            ISwitchConfiguration config;
            return _configurations.TryGetValue(switchId, out config) && config is TConfig;
        }

        public TConfig GetConfig<TConfig>(Guid switchId) where TConfig : ISwitchConfiguration
        {
            return (TConfig)_configurations[switchId];
        }

        public Guid[] GetAllIds()
        {
            return _configurations.Keys.ToArray();
        }
    }
}