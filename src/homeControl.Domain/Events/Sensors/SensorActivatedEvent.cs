namespace homeControl.Domain.Events.Sensors
{
    public class SensorActivatedEvent : AbstractSensorEvent
    {
        public SensorActivatedEvent(SensorId sensorId) : base(sensorId)
        {
        }

        public override string ToString()
        {
            return $"{nameof(SensorActivatedEvent)} {SensorId}";
        }
    }
}