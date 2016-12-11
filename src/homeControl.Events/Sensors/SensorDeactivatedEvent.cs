using homeControl.Configuration.Sensors;

namespace homeControl.Events.Sensors
{
    public class SensorDeactivatedEvent : AbstractSensorEvent
    {
        public SensorDeactivatedEvent(SensorId sensorId) : base(sensorId)
        {
        }
    }
}