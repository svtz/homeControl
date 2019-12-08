using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using homeControl.Domain;
using homeControl.Domain.Configuration.Bindings;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Switches;
using homeControl.Domain.Repositories;

namespace homeControl.ControllerService.Bindings
{
    internal sealed class BindingController : IBindingController, IBindingStateManager
    {
        private class State
        {
            private readonly Dictionary<SensorId, Dictionary<SwitchId, SwitchToSensorBinding>> _enabledBindings = new Dictionary<SensorId, Dictionary<SwitchId, SwitchToSensorBinding>>();
            private readonly Dictionary<(SwitchId, SensorId), SwitchToSensorBinding> _allBindings;

            public State(IReadOnlyCollection<SwitchToSensorBinding> bindings)
            {
                Guard.DebugAssertArgumentNotNull(bindings, nameof(bindings));

                foreach (var sensorGroup in bindings.GroupBy(b => b.SensorId))
                {
                    _enabledBindings[sensorGroup.Key] = sensorGroup.ToDictionary(x => x.SwitchId);
                }

                _allBindings = bindings.ToDictionary(b => (b.SwitchId, b.SensorId));
            }

            private SwitchToSensorBinding GetBinding(SwitchId switchId, SensorId sensorId)
            {
                if (!_allBindings.TryGetValue((switchId, sensorId), out var binding))
                {
                    throw new InvalidOperationException($"Sensor {sensorId} not found in the configuration.");
                }

                return binding;
            }

            public void Enable(SwitchId switchId, SensorId sensorId)
            {
                Guard.DebugAssertArgumentNotNull(switchId, nameof(switchId));
                Guard.DebugAssertArgumentNotNull(sensorId, nameof(sensorId));

                var binding = GetBinding(switchId, sensorId);

                var enabledSwitches = _enabledBindings[sensorId];
                if (enabledSwitches.ContainsKey(switchId)) return;

                lock (enabledSwitches)
                    enabledSwitches.Add(switchId, binding);
            }

            public void Disable(SwitchId switchId, SensorId sensorId)
            {
                Guard.DebugAssertArgumentNotNull(switchId, nameof(switchId));
                Guard.DebugAssertArgumentNotNull(sensorId, nameof(sensorId));

                var enabledSwitches = _enabledBindings[sensorId];
                if (!enabledSwitches.ContainsKey(switchId)) return;

                lock (enabledSwitches)
                    enabledSwitches.Remove(switchId);
            }

            public IReadOnlyCollection<SwitchToSensorBinding> GetAutomatedSwitches(SensorId sensorId)
            {
                Guard.DebugAssertArgumentNotNull(sensorId, nameof(sensorId));

                if (!_enabledBindings.TryGetValue(sensorId, out var switches))
                {
                    return Array.Empty<SwitchToSensorBinding>();
                }

                return switches.Values;
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
            var bindingsToTurnOn = (await GetState())
                .GetAutomatedSwitches(sensorId)
                .OfType<OnOffBinding>()
                .Where(s => s.Mode != OnOffBindingMode.OffOnly);
            foreach (var binding in bindingsToTurnOn)
            {
                _eventSender.SendEvent(new TurnOnEvent(binding.SwitchId));
            }
        }

        public async Task ProcessSensorDeactivation(SensorId sensorId)
        {
            var bindingsToTurnOff = (await GetState())
                .GetAutomatedSwitches(sensorId)
                .OfType<OnOffBinding>()
                .Where(s => s.Mode != OnOffBindingMode.OnOnly);
            foreach (var binding in bindingsToTurnOff)
            {
                _eventSender.SendEvent(new TurnOffEvent(binding.SwitchId));
            }
        }
    }
}
