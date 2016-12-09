using System;

namespace homeControl.Events
{
    public class TurnOnEvent : AbstractSwitchEvent
    {
        public TurnOnEvent(Guid switchId) : base(switchId)
        {
        }
    }
}