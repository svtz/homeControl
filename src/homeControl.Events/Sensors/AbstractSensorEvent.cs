using System;
using homeControl.Core;

namespace homeControl.Events.Sensors
{
    public abstract class AbstractSensorEvent : IEvent
    {
        public Guid SensorId { get; }

        protected AbstractSensorEvent(Guid sensorId)
        {
            Guard.DebugAssertArgumentNotDefault(sensorId, nameof(sensorId));
            SensorId = sensorId;
        }
    }
}
