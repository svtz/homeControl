using System;

namespace homeControl.Events.Switches
{
    public class TurnOffEvent : AbstractSwitchEvent
    {
        public TurnOffEvent(SwitchId switchId) : base(switchId)
        {
        }
    }
}