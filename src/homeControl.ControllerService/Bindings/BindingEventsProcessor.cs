using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Bindings;
using JetBrains.Annotations;
using Serilog;

namespace homeControl.ControllerService.Bindings
{
    [UsedImplicitly]
    internal sealed class BindingEventsProcessor
    {
        private readonly IBindingStateManager _bindingStateManager;
        private readonly IEventSource _source;
        private readonly ILogger _log;

        public BindingEventsProcessor(IBindingStateManager bindingStateManager, IEventSource source,
            ILogger log)
        {
            Guard.DebugAssertArgumentNotNull(bindingStateManager, nameof(bindingStateManager));
            Guard.DebugAssertArgumentNotNull(source, nameof(source));
            Guard.DebugAssertArgumentNotNull(log, nameof(log));

            _bindingStateManager = bindingStateManager;
            _source = source;
            _log = log;
        }
        
        public Task Run(CancellationToken ct)
        {
            return _source
                .ReceiveEvents<AbstractBindingEvent>()
                .ForEachAsync(async e => await HandleEvent(e), ct);
        }

        private async Task HandleEvent(AbstractBindingEvent bindingEvent)
        {
            Guard.DebugAssertArgumentNotNull(bindingEvent, nameof(bindingEvent));

            if (bindingEvent is EnableBindingEvent)
            {
                await _bindingStateManager.EnableBinding(bindingEvent.SwitchId, bindingEvent.SensorId);
                _log.Information("Binding enabled: {SwitchId}, {SensorId}", bindingEvent.SwitchId, bindingEvent.SensorId);
            }
            else if (bindingEvent is DisableBindingEvent)
            {
                await _bindingStateManager.DisableBinding(bindingEvent.SwitchId, bindingEvent.SensorId);
                _log.Information("Binding disabled: {SwitchId}, {SensorId}", bindingEvent.SwitchId, bindingEvent.SensorId);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
