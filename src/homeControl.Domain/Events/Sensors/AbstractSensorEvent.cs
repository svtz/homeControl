namespace homeControl.Domain.Events.Sensors
{
    public abstract class AbstractSensorEvent : IEvent
    {
        public SensorId SensorId { get; }

        protected AbstractSensorEvent(SensorId sensorId)
        {
            Guard.DebugAssertArgumentNotNull(sensorId, nameof(sensorId));
            SensorId = sensorId;
        }
    }
}
