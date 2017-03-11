using System;
using System.Collections.Generic;
using System.Linq;
using homeControl.Configuration;

namespace homeControl.ClientApi.Configuration
{
    internal sealed class ClientApiConfigurationRepository :
        AbstractConfigurationRepository<SwitchApiConfig[], Dictionary<Guid, SwitchApiConfig>>,
        IClientApiConfigurationRepository
    {
        public ClientApiConfigurationRepository(IConfigurationLoader<SwitchApiConfig[]> configLoader)
            : base(configLoader, PrepareConfiguration, "client-api.json")
        {
        }

        private static Dictionary<Guid, SwitchApiConfig> PrepareConfiguration(SwitchApiConfig[] configurations)
        {
            Guard.DebugAssertArgumentNotNull(configurations, nameof(configurations));

            if (configurations.Any(cfg => cfg == null))
                throw new InvalidConfigurationException($"Found null-configuration for client-api.");
            if (configurations.Any(cfg => cfg.ConfigId == Guid.Empty))
                throw new InvalidConfigurationException("Found zero identifier in the client-api config.");
            if (configurations.Any(cfg => cfg.SwitchId.Id == Guid.Empty))
                throw new InvalidConfigurationException("Found zero switch identifier in the client-api config.");
            if (configurations.OfType<AutomatedSwitchApiConfig>().Any(cfg => cfg.SensorId.Id == Guid.Empty))
                throw new InvalidConfigurationException("Found zero sensor identifier in the client-api config.");
            if (configurations.Any(cfg => string.IsNullOrWhiteSpace(cfg.Name)))
                throw new InvalidConfigurationException("Found empty name in the client-api config.");
            if (configurations.Any(cfg => string.IsNullOrWhiteSpace(cfg.Description)))
                throw new InvalidConfigurationException("Found empty description in the client-api config.");

            try
            {
                return configurations.ToDictionary(c => c.ConfigId);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidConfigurationException(ex, "Error while clientApi configuration loading.");
            }
        }

        public IReadOnlyCollection<SwitchApiConfig> GetAll()
        {
            return Configuration.Values;
        }

        public SwitchApiConfig TryGetById(Guid id)
        {
            SwitchApiConfig config;
            return Configuration.TryGetValue(id, out config) ? config : null;
        }
    }
}