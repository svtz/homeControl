using System;
using homeControl.Configuration.Switches;
using homeControl.Events.Switches;
using homeControl.WebApi.Dto;

namespace homeControl.WebApi.Controllers
{
    internal sealed class SetToggleSwitchValueStrategy : ISetSwitchValueStrategy
    {
        public bool CanHandle(SwitchKind switchKind, object value)
        {
            if (switchKind == SwitchKind.ToggleSwitch && value is bool)
            {
                return true;
            }

            return false;
        }

        public SetPowerEvent CreateSetPowerEvent(SwitchId id, object value)
        {
            return new SetPowerEvent(id, (bool)value ? SetPowerEvent.MaxPower : SetPowerEvent.MinPower);
        }

        public AbstractSwitchEvent CreateControlEvent(SwitchId id, object value)
        {
            var b = (bool)value;
            return b
                ? (AbstractSwitchEvent)new TurnOnEvent(id) 
                : (AbstractSwitchEvent)new TurnOffEvent(id);
        }
    }
}