namespace homeControl.Domain.Events.Switches
{
    public class TurnSwitchOffEvent : AbstractSwitchEvent
    {
        public TurnSwitchOffEvent(SwitchId switchId) : base(switchId)
        {
        }

        public override string ToString()
        {
            return $"{nameof(TurnSwitchOffEvent)} {SwitchId}";
        }
    }
}