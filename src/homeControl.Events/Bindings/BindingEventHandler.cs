using System;
using homeControl.Core;

namespace homeControl.Events.Bindings
{
    internal sealed class BindingEventHandler : IHandler
    {
        private readonly IBindingStateManager _bindingStateManager;

        public BindingEventHandler(IBindingStateManager bindingStateManager)
        {
            _bindingStateManager = bindingStateManager;
        }
        
        public bool CanHandle(IEvent @event)
        {
            Guard.DebugAssertArgumentNotNull(@event, nameof(@event));
            return @event is AbstractBindingEvent;
        }

        public void Handle(IEvent @event)
        {
            Guard.DebugAssertArgumentNotNull(@event, nameof(@event));
            Guard.DebugAssertArgument(CanHandle(@event), nameof(@event));

            var bindingEvent = (AbstractBindingEvent)@event;
            if (bindingEvent is EnableBindingEvent)
            {
                _bindingStateManager.EnableBinding(bindingEvent.SwitchId, bindingEvent.SensorId);
            }
            else if (@event is DisableBindingEvent)
            {
                _bindingStateManager.DisableBinding(bindingEvent.SwitchId, bindingEvent.SensorId);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
