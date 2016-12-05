using System;

namespace homeControl.Core.Events
{
    public class TurnOffEvent : AbstractSwitchEvent
    {
        public TurnOffEvent(Guid switchId) : base(switchId)
        {
        }
    }
}