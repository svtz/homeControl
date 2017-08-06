namespace homeControl.Domain.Events.Sensors
{
    public class SensorDeactivatedEvent : AbstractSensorEvent
    {
        public SensorDeactivatedEvent(SensorId sensorId) : base(sensorId)
        {
        }
    }
}