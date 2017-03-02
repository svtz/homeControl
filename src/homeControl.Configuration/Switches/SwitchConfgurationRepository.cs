using System;
using System.Collections.Generic;
using System.Linq;

namespace homeControl.Configuration.Switches
{
    internal sealed class SwitchConfgurationRepository :
        AbstractConfigurationRepository<ISwitchConfiguration[], Dictionary<SwitchId, ISwitchConfiguration>>,
        ISwitchConfigurationRepository
    {
        public SwitchConfgurationRepository(IConfigurationLoader<ISwitchConfiguration[]> configLoader)
            : base(configLoader, PrepareConfiguration, "switches.json")
        {
        }

        private static Dictionary<SwitchId, ISwitchConfiguration> PrepareConfiguration(ISwitchConfiguration[] configurations)
        {
            Guard.DebugAssertArgumentNotNull(configurations, nameof(configurations));

            if (configurations.Any(cfg => cfg == null))
                throw new InvalidConfigurationException($"Found null-configuration for switch.");
            if (configurations.Any(cfg => cfg.SwitchId?.Id == Guid.Empty))
                throw new InvalidConfigurationException("Found zero identifier in the switch config.");

            try
            {
                return configurations.ToDictionary(cfg => cfg.SwitchId);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidConfigurationException(ex, "Found duplicated switch ids in the configuration file.");
            }
        }

        public bool ContainsConfig<TConfig>(SwitchId switchId) where TConfig : ISwitchConfiguration
        {
            ISwitchConfiguration config;
            return Configuration.TryGetValue(switchId, out config) && config is TConfig;
        }

        public TConfig GetConfig<TConfig>(SwitchId switchId) where TConfig : ISwitchConfiguration
        {
            return (TConfig)Configuration[switchId];
        }

        public IReadOnlyCollection<ISwitchConfiguration> GetAll()
        {
            return Configuration.Values;
        }
    }
}