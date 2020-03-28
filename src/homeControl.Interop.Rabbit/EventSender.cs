using System.Threading.Tasks;
using homeControl.Domain.Events;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NServiceBus;
using Serilog;
using Serilog.Events;
using IEvent = homeControl.Domain.Events.IEvent;

namespace homeControl.Interop.Rabbit
{
    [UsedImplicitly]
    internal sealed class EventSender : IEventSender
    {
        private readonly IEndpointInstance _endpoint;
        private readonly ILogger _logger;

        public EventSender(IEndpointInstance endpoint, ILogger logger)
        {
            Guard.DebugAssertArgumentNotNull(endpoint, nameof(endpoint));

            _endpoint = endpoint;
            _logger = logger;
        }

        public async Task SendEvent(IEvent @event)
        {
            Guard.DebugAssertArgumentNotNull(@event, nameof(@event));

            if (_logger.IsEnabled(LogEventLevel.Verbose))
            {
                _logger.Verbose(">>> {messageType}: {message}",
                    @event.GetType(),
                    JsonConvert.SerializeObject(@event, Formatting.Indented));
            }

            await _endpoint.Publish(@event);
        }
    }
}