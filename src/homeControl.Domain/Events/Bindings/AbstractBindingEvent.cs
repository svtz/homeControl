namespace homeControl.Domain.Events.Bindings
{
    public abstract class AbstractBindingEvent : IEvent
    {
        public SwitchId SwitchId { get; }
        public SensorId SensorId { get; }

        protected AbstractBindingEvent(SwitchId switchId, SensorId sensorId)
        {
            SwitchId = switchId;
            SensorId = sensorId;
        }
    }
}