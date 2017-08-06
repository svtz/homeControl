using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Bindings;
using JetBrains.Annotations;

namespace homeControl.ControllerService.Bindings
{
    [UsedImplicitly]
    internal sealed class BindingEventsProcessor
    {
        private readonly IBindingStateManager _bindingStateManager;
        private readonly IEventSource _source;

        public BindingEventsProcessor(IBindingStateManager bindingStateManager, IEventSource source)
        {
            Guard.DebugAssertArgumentNotNull(bindingStateManager, nameof(bindingStateManager));
            Guard.DebugAssertArgumentNotNull(source, nameof(source));

            _bindingStateManager = bindingStateManager;
            _source = source;
        }
        
        public Task Run(CancellationToken ct)
        {
            return _source.ReceiveEvents<AbstractBindingEvent>().ForEachAsync(HandleEvent, ct);
        }

        private void HandleEvent(AbstractBindingEvent bindingEvent)
        {
            if (bindingEvent is EnableBindingEvent)
            {
                _bindingStateManager.EnableBinding(bindingEvent.SwitchId, bindingEvent.SensorId);
            }
            else if (bindingEvent is DisableBindingEvent)
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
