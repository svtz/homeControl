namespace homeControl.Domain.Events.Sensors
{
    public class SensorIncreasePowerEvent : AbstractSensorEvent
    {
        public SensorIncreasePowerEvent(SensorId sensorId) : base(sensorId)
        {
        }

        public override string ToString()
        {
            return $"{nameof(SensorIncreasePowerEvent)} {SensorId}";
        }
    }
}