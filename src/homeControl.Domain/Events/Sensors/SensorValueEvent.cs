namespace homeControl.Domain.Events.Sensors
{
    public class SensorValueEvent : AbstractSensorEvent
    {
        public SensorValueEvent(SensorId sensorId, decimal value) : base(sensorId)
        {
            Value = value;
        }
        
        public decimal Value { get; }
        
        public override string ToString()
        {
            return $"{nameof(SensorValueEvent)} {SensorId} {Value}";
        }
    }
}