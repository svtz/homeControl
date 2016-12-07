using System;
using homeControl.Core.Misc;

namespace homeControl.Core.Events
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
