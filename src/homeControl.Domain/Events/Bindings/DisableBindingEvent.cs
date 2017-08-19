namespace homeControl.Domain.Events.Bindings
{
    public class DisableBindingEvent : AbstractBindingEvent
    {
        public DisableBindingEvent(SwitchId switchId, SensorId sensorId) : base(switchId, sensorId)
        {
        }

        public override string ToString()
        {
            return $"{nameof(DisableBindingEvent)} {SwitchId}::{SensorId}";
        }
    }
}