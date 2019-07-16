using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using homeControl.Configuration;
using homeControl.Domain;
using JetBrains.Annotations;
using Serilog;

namespace homeControl.NooliteF.Configuration
{
    [UsedImplicitly]
    internal sealed class NooliteFSwitchInfoRepository :
        AbstractConfigurationRepository<NooliteFSwitchInfo[], Dictionary<SwitchId, NooliteFSwitchInfo>>, INooliteFSwitchInfoRepository
    {
        public NooliteFSwitchInfoRepository(IConfigurationLoader<NooliteFSwitchInfo[]> configLoader, ILogger logger)
            : base("switches-noolite-f", configLoader, PrepareConfiguration, logger)
        {
        }

        private static Dictionary<SwitchId, NooliteFSwitchInfo> PrepareConfiguration(NooliteFSwitchInfo[] configurations)
        {
            Guard.DebugAssertArgumentNotNull(configurations, nameof(configurations));

            if (configurations.Any(cfg => cfg == null))
                throw new InvalidConfigurationException($"Found null-configuration for NooliteSwitchInfo.");
            if (configurations.Any(cfg => cfg.SwitchId?.Id == Guid.Empty))
                throw new InvalidConfigurationException("Found zero identifier in the NooliteSwitchInfo config.");

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

        public async Task<NooliteFSwitchInfo> GetConfig(SwitchId switchId)
        {
            return (await GetConfiguration())[switchId];
        }

        public async Task<IReadOnlyCollection<NooliteFSwitchInfo>> GetAll()
        {
            return (await GetConfiguration()).Values;
        }
    }
}