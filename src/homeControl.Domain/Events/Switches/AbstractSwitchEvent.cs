using homeControl.Configuration.Switches;
using homeControl.Domain.Events;

namespace homeControl.Events.Switches
{
    public abstract class AbstractSwitchEvent : IEvent
    {
        public SwitchId SwitchId { get; }

        protected AbstractSwitchEvent(SwitchId switchId)
        {
            Guard.DebugAssertArgumentNotNull(switchId, nameof(switchId));
            SwitchId = switchId;
        }
    }
}
