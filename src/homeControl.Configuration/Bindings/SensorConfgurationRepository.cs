using System.Linq;

namespace homeControl.Configuration.Bindings
{
    internal sealed class SwitchToSensorBindingsRepository : ISwitchToSensorBindingsRepository
    {
        private readonly ISwitchToSensorBinding[] _bindings;

        public SwitchToSensorBindingsRepository(ISwitchToSensorBinding[] bindings)
        {
            Guard.DebugAssertArgumentNotNull(bindings, nameof(bindings));
            if (bindings.Any(cfg => cfg == null))
                throw new InvalidConfigurationException("Found null-configuration for switch-to-sensor binding.");

            _bindings = bindings;
        }

        public ISwitchToSensorBinding[] GetAll()
        {
            return _bindings;
        }
    }
}