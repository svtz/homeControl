using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using homeControl.Domain;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Switches;
using homeControl.Domain.Repositories;

namespace homeControl.ControllerService.Bindings
{
    internal sealed class BindingController : IBindingController, IBindingStateManager
    {
        private class State
        {
            private readonly Dictionary<SensorId, HashSet<SwitchId>> _enabledBindings = new Dictionary<SensorId, HashSet<SwitchId>>();
            private readonly HashSet<Tuple<SwitchId, SensorId>> _allowedCombinations;

            public State(IReadOnlyCollection<SwitchToSensorBinding> bindings)
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

        private readonly IEventSender _eventSender;
        private readonly ISwitchToSensorBindingsRepository _bindingsRepository;

        private readonly object _lock = new object();
        private volatile Task<State> _stateCreator;
        private Task<State> GetState()
        {
            if (_stateCreator != null)
                return _stateCreator;

            lock (_lock)
            {
                if (_stateCreator != null)
                    return _stateCreator;

                _stateCreator = Task.Run(async () =>
                {
                    var bindings = await _bindingsRepository.GetAll();
                    return new State(bindings);
                });
            }

            return _stateCreator;
        }

        public BindingController(IEventSender eventSender, ISwitchToSensorBindingsRepository bindingsRepository)
        {
            Guard.DebugAssertArgumentNotNull(eventSender, nameof(eventSender));
            Guard.DebugAssertArgumentNotNull(bindingsRepository, nameof(bindingsRepository));

            _eventSender = eventSender;
            _bindingsRepository = bindingsRepository;
        }

        public async Task EnableBinding(SwitchId switchId, SensorId sensorId)
        {
            (await GetState()).Enable(switchId, sensorId);
        }

        public async Task DisableBinding(SwitchId switchId, SensorId sensorId)
        {
            (await GetState()).Disable(switchId, sensorId);
        }

        public async Task ProcessSensorActivation(SensorId sensorId)
        {
            var switchesToTurnOn = (await GetState()).GetAutomatedSwitches(sensorId);
            foreach (var switchId in switchesToTurnOn)
            {
                _eventSender.SendEvent(new TurnOnEvent(switchId));
            }
        }

        public async Task ProcessSensorDeactivation(SensorId sensorId)
        {
            var switchesToTurnOff = (await GetState()).GetAutomatedSwitches(sensorId);
            foreach (var switchId in switchesToTurnOff)
            {
                _eventSender.SendEvent(new TurnOffEvent(switchId));
            }
        }
    }
}
