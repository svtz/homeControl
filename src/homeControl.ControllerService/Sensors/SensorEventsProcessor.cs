using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using homeControl.Configuration;
using homeControl.ControllerService.Bindings;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Sensors;
using JetBrains.Annotations;
using Serilog;

namespace homeControl.ControllerService.Sensors
{
    [UsedImplicitly]
    internal sealed class SensorEventsProcessor
    {
        private readonly IBindingController _bindingController;
        private readonly IEventReceiver _receiver;
        private readonly ILogger _log;

        public SensorEventsProcessor(IBindingController bindingController, IEventReceiver receiver,
            ILogger log)
        {
            Guard.DebugAssertArgumentNotNull(bindingController, nameof(bindingController));
            Guard.DebugAssertArgumentNotNull(log, nameof(log));
            Guard.DebugAssertArgumentNotNull(receiver, nameof(receiver));

            _bindingController = bindingController;
            _receiver = receiver;
            _log = log;
        }

        public Task Run(CancellationToken ct)
        {
            return _receiver
                .ReceiveEvents<AbstractSensorEvent>()
                .ForEachAsyncAsync(async e => await Handle(e), ct);
        }

        private async Task Handle(AbstractSensorEvent sensorEvent)
        {
            Guard.DebugAssertArgumentNotNull(sensorEvent, nameof(sensorEvent));

            if (sensorEvent is SensorActivatedEvent)
            {
                _log.Information("Sensor {SensorId} activated", sensorEvent.SensorId);
                await _bindingController.ProcessSensorActivation(sensorEvent.SensorId);
            }
            else if (sensorEvent is SensorDeactivatedEvent)
            {
                _log.Information("Sensor {SensorId} deactivated", sensorEvent.SensorId);
                await _bindingController.ProcessSensorDeactivation(sensorEvent.SensorId);
            }
            else if (sensorEvent is SensorValueEvent valueEvent)
            {
                _log.Information("Sensor {SensorId} value={value}", sensorEvent.SensorId, valueEvent.Value);
                await _bindingController.ProcessSensorValue(valueEvent.SensorId, valueEvent.Value);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
