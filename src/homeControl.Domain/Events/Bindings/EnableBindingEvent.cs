namespace homeControl.Domain.Events.Bindings
{
    public class EnableBindingEvent : AbstractBindingEvent
    {
        public EnableBindingEvent(SwitchId switchId, SensorId sensorId) : base(switchId, sensorId)
        {
        }

        public override string ToString()
        {
            return $"{nameof(EnableBindingEvent)} {SwitchId}::{SensorId}";
        }
    }
}