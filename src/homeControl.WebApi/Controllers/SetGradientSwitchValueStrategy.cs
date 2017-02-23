using System;
using homeControl.Configuration.Switches;
using homeControl.Events.Switches;
using homeControl.WebApi.Dto;

namespace homeControl.WebApi.Controllers
{
    internal sealed class SetGradientSwitchValueStrategy : ISetSwitchValueStrategy
    {
        public bool CanHandle(SwitchKind switchKind, object value)
        {
            if (switchKind == SwitchKind.GradientSwitch && value is double)
            {
                var d = (double)value;
                if (d >= SetPowerEvent.MinPower && d <= SetPowerEvent.MaxPower)
                    return true;
            }

            return false;
        }


        public SetPowerEvent CreateSetPowerEvent(SwitchId id, object value)
        {
            return new SetPowerEvent(id, (double)value);
        }

        public AbstractSwitchEvent CreateControlEvent(SwitchId id, object value)
        {
            var d = (double)value;
            if (AreEqual(d, SetPowerEvent.MinPower))
            {
                return new TurnOffEvent(id);
            }

            return new TurnOnEvent(id);
        }

        private static bool AreEqual(double a, double b)
        {
            return Math.Abs(a - b) < double.Epsilon;
        }
    }
}