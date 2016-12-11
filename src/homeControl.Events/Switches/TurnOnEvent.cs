using System;
using homeControl.Configuration.Switches;

namespace homeControl.Events.Switches
{
    public class TurnOnEvent : AbstractSwitchEvent
    {
        public TurnOnEvent(SwitchId switchId) : base(switchId)
        {
        }
    }
}