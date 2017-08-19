namespace homeControl.Domain.Events.Switches
{
    public class TurnOnEvent : AbstractSwitchEvent
    {
        public TurnOnEvent(SwitchId switchId) : base(switchId)
        {
        }

        public override string ToString()
        {
            return $"{nameof(TurnOnEvent)} {SwitchId}";
        }
    }
}