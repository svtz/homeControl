using System;
using System.Collections.Generic;
using System.Linq;
using homeControl.Domain;
using homeControl.Domain.Repositories;
using JetBrains.Annotations;

namespace homeControl.Configuration
{
    [UsedImplicitly]
    internal sealed class SwitchToSensorBindingsRepository : 
        AbstractConfigurationRepository<ISwitchToSensorBinding[], ISwitchToSensorBinding[]>,
        ISwitchToSensorBindingsRepository
    {
        public SwitchToSensorBindingsRepository(IConfigurationLoader<ISwitchToSensorBinding[]> configLoader)
            : base("bindings", configLoader, PrepareConfiguration)
        {
        }

        private static ISwitchToSensorBinding[] PrepareConfiguration(ISwitchToSensorBinding[] configurations)
        {
            Guard.DebugAssertArgumentNotNull(configurations, nameof(configurations));

            if (configurations.Any(cfg => cfg == null))
                throw new InvalidConfigurationException("Found null-configuration for switch-to-sensor binding.");
            if (configurations.Any(cfg => cfg.SensorId?.Id == Guid.Empty))
                throw new InvalidConfigurationException("Found zero sensor identifier in the binding config.");
            if (configurations.Any(cfg => cfg.SwitchId?.Id == Guid.Empty))
                throw new InvalidConfigurationException("Found zero switch identifier in the binding config.");

            return configurations;
        }

        public IReadOnlyCollection<ISwitchToSensorBinding> GetAll()
        {
            return Configuration;
        }
    }
}