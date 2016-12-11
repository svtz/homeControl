using homeControl.Configuration.Sensors;
using homeControl.Core;

namespace homeControl.Events.Sensors
{
    public abstract class AbstractSensorEvent : IEvent
    {
        public SensorId SensorId { get; }

        protected AbstractSensorEvent(SensorId sensorId)
        {
            Guard.DebugAssertArgumentNotNull(sensorId, nameof(sensorId));
            SensorId = sensorId;
        }
    }
}
