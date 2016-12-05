using System;
using homeControl.Core.Misc;

namespace homeControl.Core.Events
{
    public abstract class AbstractSwitchEvent : IEvent
    {
        public Guid SwitchId { get; }

        protected AbstractSwitchEvent(Guid switchId)
        {
            Guard.DebugAssertArgument(switchId != Guid.Empty, nameof(switchId));
            SwitchId = switchId;
        }
    }
}
