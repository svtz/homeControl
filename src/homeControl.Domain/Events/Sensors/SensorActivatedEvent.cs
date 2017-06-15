using homeControl.Configuration.Sensors;

namespace homeControl.Events.Sensors
{
    public class SensorActivatedEvent : AbstractSensorEvent
    {
        public SensorActivatedEvent(SensorId sensorId) : base(sensorId)
        {
        }
    }
}