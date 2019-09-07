using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using homeControl.Domain;
using homeControl.Domain.Events;
using homeControl.Domain.Repositories;
using JetBrains.Annotations;
using Serilog;

namespace homeControl.Client.WPF.ViewModels.Switches
{
    [UsedImplicitly]
    public sealed class SwitchViewModelsFactory
    {
        private readonly IEventSource _eventSource;
        private readonly IEventSender _eventSender;
        private readonly ISwitchConfigurationRepository _switchesRepo;
        private readonly ISwitchToSensorBindingsRepository _bindingsRepo;
        private readonly ILogger _log;

        public SwitchViewModelsFactory(
            IEventSource eventSource,
            IEventSender eventSender,
            ISwitchConfigurationRepository switchesRepo,
            ISwitchToSensorBindingsRepository bindingsRepo,
            ILogger log)
        {
            Guard.DebugAssertArgumentNotNull(eventSender, nameof(eventSender));
            Guard.DebugAssertArgumentNotNull(eventSource, nameof(eventSource));
            Guard.DebugAssertArgumentNotNull(switchesRepo, nameof(switchesRepo));
            Guard.DebugAssertArgumentNotNull(bindingsRepo, nameof(bindingsRepo));
            Guard.DebugAssertArgumentNotNull(log, nameof(log));

            _eventSource = eventSource;
            _eventSender = eventSender;
            _switchesRepo = switchesRepo;
            _bindingsRepo = bindingsRepo;
            _log = log;
        }

        public async Task<SwitchViewModelBase[]> CreateViewModels()
        {
            var bindings = await _bindingsRepo.GetAll();
            var switches = (await _switchesRepo.GetAll()).Where(s => s.ShowOnUi).ToArray();
            var sensorsBySwitches = bindings
                .GroupBy(b => b.SwitchId, b => b.SensorId)
                .ToDictionary(b => b.Key, b => b.ToArray());

            var result = new List<SwitchViewModelBase>(switches.Length);
            foreach (var @switch in switches)
            {
                SwitchViewModelBase vm;
                SensorId[] sensors;
                if (!sensorsBySwitches.TryGetValue(@switch.SwitchId, out sensors))
                    sensors = new SensorId[0];
                
                switch (@switch.SwitchKind)
                {
                    case SwitchKind.Toggle:
                        vm = new ToggleSwitchViewModel(_eventSource, _eventSender, sensors, _log);
                        break;

                    case SwitchKind.Gradient:
                        vm = new GradientSwitchViewModel(_eventSource, _eventSender, sensors, _log);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(@switch.SwitchKind));
                }

                vm.Id = @switch.SwitchId;
                vm.Name = @switch.Name;
                vm.Description = @switch.Description;
                result.Add(vm);
            }

            return result.ToArray();
        }
    }
}