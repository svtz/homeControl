namespace homeControl.Domain.Events.Switches
{
    public class SetPowerEvent : AbstractSwitchEvent
    {
        public const double MaxPower = 1.0;
        public const double MinPower = 0.0;

        public double Power { get; }

        public SetPowerEvent(SwitchId switchId, double power) : base(switchId)
        {
            Guard.DebugAssertArgument(power >= MinPower && power <= MaxPower, nameof(power));
            Power = power;
        }
    }
}