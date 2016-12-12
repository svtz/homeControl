using homeControl.Configuration.Sensors;
using homeControl.Core;
using homeControl.Peripherals;

namespace homeControl.Events.Sensors
{
    internal sealed class SensorGate : ISensorGate
    {
        private readonly IEventPublisher _eventPublisher;

        public SensorGate(IEventPublisher eventPublisher)
        {
            Guard.DebugAssertArgumentNotNull(eventPublisher, nameof(eventPublisher));

            _eventPublisher = eventPublisher;
        }

        public void OnSensorActivated(SensorId sensorId)
        {
            Guard.DebugAssertArgumentNotNull(sensorId, nameof(sensorId));

            _eventPublisher.PublishEvent(new SensorActivatedEvent(sensorId));
        }

        public void OnSensorDeactivated(SensorId sensorId)
        {
            Guard.DebugAssertArgumentNotNull(sensorId, nameof(sensorId));

            _eventPublisher.PublishEvent(new SensorDeactivatedEvent(sensorId));
        }
    }
}
