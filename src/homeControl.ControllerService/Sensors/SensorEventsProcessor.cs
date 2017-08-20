using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly IEventSource _source;
        private readonly ILogger _log;

        public SensorEventsProcessor(IBindingController bindingController, IEventSource source,
            ILogger log)
        {
            Guard.DebugAssertArgumentNotNull(bindingController, nameof(bindingController));
            Guard.DebugAssertArgumentNotNull(log, nameof(log));
            Guard.DebugAssertArgumentNotNull(source, nameof(source));

            _bindingController = bindingController;
            _source = source;
            _log = log;
        }

        public Task Run(CancellationToken ct)
        {
            return _source
                .ReceiveEvents<AbstractSensorEvent>()
                .ForEachAsync(async e => await Handle(e), ct);
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
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
