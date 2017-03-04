using System;
using System.Collections.Generic;
using System.Linq;
using homeControl.Configuration.Bindings;
using homeControl.Configuration.Sensors;
using homeControl.Configuration.Switches;
using homeControl.Core;
using homeControl.Events.Switches;

namespace homeControl.Events.Bindings
{
    internal sealed class BindingController : IBindingStateManager, IBindingController
    {
        private class State
        {
            private readonly Dictionary<SensorId, HashSet<SwitchId>> _enabledBindings = new Dictionary<SensorId, HashSet<SwitchId>>();
            private readonly HashSet<Tuple<SwitchId, SensorId>> _allowedCombinations;

            public State(IReadOnlyCollection<ISwitchToSensorBinding> bindings)
            {
                Guard.DebugAssertArgumentNotNull(bindings, nameof(bindings));

                foreach (var sensorGroup in bindings.GroupBy(b => b.SensorId))
                {
                    var sensorBindings = sensorGroup.Select(binding => binding.SwitchId);
                    _enabledBindings[sensorGroup.Key] = new HashSet<SwitchId>(sensorBindings);
                }

                var allowedCombinations = bindings.Select(b => Tuple.Create(b.SwitchId, b.SensorId));
                _allowedCombinations = new HashSet<Tuple<SwitchId, SensorId>>(allowedCombinations);
            }

            private void CheckBindingIsValid(SwitchId switchId, SensorId sensorId)
            {
                if (!_enabledBindings.ContainsKey(sensorId) ||
                    !_allowedCombinations.Contains(Tuple.Create(switchId, sensorId)))
                {
                    throw new InvalidOperationException($"Sensor {sensorId} not found in the configuration.");
                }
            }

            public void Enable(SwitchId switchId, SensorId sensorId)
            {
                Guard.DebugAssertArgumentNotNull(switchId, nameof(switchId));
                Guard.DebugAssertArgumentNotNull(sensorId, nameof(sensorId));

                CheckBindingIsValid(switchId, sensorId);

                var enabledSwitches = _enabledBindings[sensorId];
                if (enabledSwitches.Contains(switchId)) return;

                lock (enabledSwitches)
                    enabledSwitches.Add(switchId);
            }

            public void Disable(SwitchId switchId, SensorId sensorId)
            {
                Guard.DebugAssertArgumentNotNull(switchId, nameof(switchId));
                Guard.DebugAssertArgumentNotNull(sensorId, nameof(sensorId));

                CheckBindingIsValid(switchId, sensorId);

                var enabledSwitches = _enabledBindings[sensorId];
                if (!enabledSwitches.Contains(switchId)) return;

                lock (enabledSwitches)
                    enabledSwitches.Remove(switchId);
            }

            public IReadOnlyCollection<SwitchId> GetAutomatedSwitches(SensorId sensorId)
            {
                Guard.DebugAssertArgumentNotNull(sensorId, nameof(sensorId));

                HashSet<SwitchId> switches;
                if (!_enabledBindings.TryGetValue(sensorId, out switches))
                {
                    return Array.Empty<SwitchId>();
                }

                return switches;
            }
        }

        private readonly IEventPublisher _eventPublisher;
        private readonly Lazy<State> _state;


        public BindingController(IEventPublisher eventPublisher, ISwitchToSensorBindingsRepository bindingsRepository)
        {
            _eventPublisher = eventPublisher;

            _state = new Lazy<State>(() => new State(bindingsRepository.GetAll()));
        }

        public void EnableBinding(SwitchId switchId, SensorId sensorId)
        {
            _state.Value.Enable(switchId, sensorId);
        }

        public void DisableBinding(SwitchId switchId, SensorId sensorId)
        {
            _state.Value.Disable(switchId, sensorId);
        }

        public void ProcessSensorActivation(SensorId sensorId)
        {
            var switchesToTurnOn = _state.Value.GetAutomatedSwitches(sensorId);
            foreach (var switchId in switchesToTurnOn)
            {
                _eventPublisher.PublishEvent(new TurnOnEvent(switchId));
            }
        }

        public void ProcessSensorDeactivation(SensorId sensorId)
        {
            var switchesToTurnOff = _state.Value.GetAutomatedSwitches(sensorId);
            foreach (var switchId in switchesToTurnOff)
            {
                _eventPublisher.PublishEvent(new TurnOffEvent(switchId));
            }
        }
    }
}
