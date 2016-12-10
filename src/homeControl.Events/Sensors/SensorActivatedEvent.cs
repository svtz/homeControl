using System;

namespace homeControl.Events.Sensors
{
    public class SensorActivatedEvent : AbstractSensorEvent
    {
        public SensorActivatedEvent(Guid sensorId) : base(sensorId)
        {
        }
    }
}