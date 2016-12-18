using homeControl.Configuration.Switches;

namespace homeControl.Events.Switches
{
    public class SetPowerEvent : AbstractSwitchEvent
    {
        public double Power { get; }

        public SetPowerEvent(SwitchId switchId, double power) : base(switchId)
        {
            Guard.DebugAssertArgument(power >= 0 && power <= 1.0, nameof(power));
            Power = power;
        }
    }
}