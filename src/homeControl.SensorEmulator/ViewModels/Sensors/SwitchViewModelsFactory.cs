using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using homeControl.Domain.Events;
using homeControl.Domain.Repositories;
using JetBrains.Annotations;
using Serilog;

namespace homeControl.SensorEmulator.ViewModels.Sensors
{
    [UsedImplicitly]
    public sealed class SwitchViewModelsFactory
    {
        private readonly IEventSource _eventSource;
        private readonly IEventSender _eventSender;
        private readonly ISensorConfigurationRepository _sensorConfigurationRepository;
        private readonly ILogger _log;

        public SwitchViewModelsFactory(
            IEventSource eventSource,
            IEventSender eventSender,
            ISensorConfigurationRepository sensorConfigurationRepository,
            ILogger log)
        {
            Guard.DebugAssertArgumentNotNull(eventSender, nameof(eventSender));
            Guard.DebugAssertArgumentNotNull(eventSource, nameof(eventSource));
            Guard.DebugAssertArgumentNotNull(sensorConfigurationRepository, nameof(sensorConfigurationRepository));
            Guard.DebugAssertArgumentNotNull(log, nameof(log));

            _eventSource = eventSource;
            _eventSender = eventSender;
            _sensorConfigurationRepository = sensorConfigurationRepository;
            _log = log;
        }

        public async Task<SensorViewModelBase[]> CreateViewModels()
        {
            var sensors = await _sensorConfigurationRepository.GetAll();

            var result = new List<SensorViewModelBase>(sensors.Count);
            foreach (var sensor in sensors)
            {
                var vm = new ToggleSensorViewModel(_eventSource, _eventSender,
                    _log.ForContext<ToggleSensorViewModel>()) {Id = sensor.SensorId};

                result.Add(vm);
            }

            return result.ToArray();
        }
    }
}