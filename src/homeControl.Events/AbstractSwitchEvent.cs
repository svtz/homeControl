using System;
using homeControl.Core;

namespace homeControl.Events
{
    public abstract class AbstractSwitchEvent : IEvent
    {
        public Guid SwitchId { get; }

        protected AbstractSwitchEvent(Guid switchId)
        {
            Guard.DebugAssertArgumentNotDefault(switchId, nameof(switchId));
            SwitchId = switchId;
        }
    }
}
