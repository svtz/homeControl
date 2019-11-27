namespace homeControl.Domain.Events.Sensors
{
    public class SensorInvertedEvent : AbstractSensorEvent
    {
        public SensorInvertedEvent(SensorId sensorId) : base(sensorId)
        {
        }

        public override string ToString()
        {
            return $"{nameof(SensorInvertedEvent)} {SensorId}";
        }
    }
}