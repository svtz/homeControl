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
    internal sealed class SwitchConfgurationRepository :
        AbstractConfigurationRepository<SwitchConfiguration[], Dictionary<SwitchId, SwitchConfiguration>>,
        ISwitchConfigurationRepository
    {
        public SwitchConfgurationRepository(IConfigurationLoader<SwitchConfiguration[]> configLoader, ILogger logger)
            : base("switches", configLoader, PrepareConfiguration, logger)
        {
        }

        private static Dictionary<SwitchId, SwitchConfiguration> PrepareConfiguration(SwitchConfiguration[] configurations)
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

        public async Task<bool> ContainsConfig(SwitchId switchId)
        {
            return (await GetConfiguration()).ContainsKey(switchId);
        }

        public async Task<SwitchConfiguration> GetConfig(SwitchId switchId)
        {
            return (await GetConfiguration())[switchId];
        }

        public async Task<IReadOnlyCollection<SwitchConfiguration>> GetAll()
        {
            return (await GetConfiguration()).Values;
        }
    }
}