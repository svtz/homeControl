using System;

namespace homeControl.Events
{
    public class TurnOffEvent : AbstractSwitchEvent
    {
        public TurnOffEvent(Guid switchId) : base(switchId)
        {
        }
    }
}