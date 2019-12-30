namespace homeControl.Domain.Events.Switches
{
    public class TurnSwitchOnEvent : AbstractSwitchEvent
    {
        public TurnSwitchOnEvent(SwitchId switchId) : base(switchId)
        {
        }

        public override string ToString()
        {
            return $"{nameof(TurnSwitchOnEvent)} {SwitchId}";
        }
    }
}