using System;

namespace homeControl.Events.Switches
{
    public class TurnOnEvent : AbstractSwitchEvent
    {
        public TurnOnEvent(Guid switchId) : base(switchId)
        {
        }
    }
}