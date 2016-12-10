using System;

namespace homeControl.Events.Sensors
{
    public class SensorDeactivatedEvent : AbstractSensorEvent
    {
        public SensorDeactivatedEvent(Guid sensorId) : base(sensorId)
        {
        }
    }
}