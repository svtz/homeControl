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

        public SetPowerEvent CreateSetPowerEvent(Guid id, object value)
        {
            return new SetPowerEvent(new SwitchId(id), (bool)value ? SetPowerEvent.MaxPower : SetPowerEvent.MinPower);
        }

        public AbstractSwitchEvent CreateControlEvent(Guid id, object value)
        {
            var b = (bool)value;
            return b
                ? (AbstractSwitchEvent)new TurnOnEvent(new SwitchId(id)) 
                : (AbstractSwitchEvent)new TurnOffEvent(new SwitchId(id));
        }
    }
}