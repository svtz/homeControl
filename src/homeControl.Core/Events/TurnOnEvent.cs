using System;

namespace homeControl.Core.Events
{
    public class TurnOnEvent : AbstractSwitchEvent
    {
        public TurnOnEvent(Guid switchId) : base(switchId)
        {
        }
    }
}