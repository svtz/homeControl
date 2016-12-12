using System;
using homeControl.Configuration.Sensors;
using homeControl.Core;

namespace homeControl.Events.Sensors
{
    internal interface ISensorWatcher
    {
        void OnSensorActivated(SensorId sensorId);
        void OnSensorDeactivated(SensorId sensorId);
    }

    internal sealed class SensorWatcher : ISensorWatcher
    {
        private readonly IEventPublisher _eventPublisher;

        public SensorWatcher(IEventPublisher eventPublisher)
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
