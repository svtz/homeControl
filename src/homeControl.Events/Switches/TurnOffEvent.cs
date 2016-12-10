using System;

namespace homeControl.Events.Switches
{
    public class TurnOffEvent : AbstractSwitchEvent
    {
        public TurnOffEvent(Guid switchId) : base(switchId)
        {
        }
    }
}