namespace homeControl.Domain.Events.Sensors
{
    public class SensorDecreasePowerEvent : AbstractSensorEvent
    {
        public SensorDecreasePowerEvent(SensorId sensorId) : base(sensorId)
        {
        }

        public override string ToString()
        {
            return $"{nameof(SensorDecreasePowerEvent)} {SensorId}";
        }
    }
}