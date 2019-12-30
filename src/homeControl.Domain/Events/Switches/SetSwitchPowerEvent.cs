namespace homeControl.Domain.Events.Switches
{
    public class SetSwitchPowerEvent : AbstractSwitchEvent
    {
        public const double MaxPower = 1.0;
        public const double MinPower = 0.0;

        public double Power { get; }

        public SetSwitchPowerEvent(SwitchId switchId, double power) : base(switchId)
        {
            Guard.DebugAssertArgument(power >= MinPower && power <= MaxPower, nameof(power));
            Power = power;
        }

        public override string ToString()
        {
            return $"{nameof(SetSwitchPowerEvent)} {SwitchId} {Power:F2}";
        }
    }
}