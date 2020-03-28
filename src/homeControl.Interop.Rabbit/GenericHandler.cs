using System;
using System.Threading.Tasks;
using homeControl.Domain.Events;
using NServiceBus;
using Serilog;
using IEvent = homeControl.Domain.Events.IEvent;

namespace homeControl.Interop.Rabbit
{
    public sealed class GenericHandler : IHandleMessages<IEvent>
    {
        private readonly EventSource _eventSource;
        private readonly ILogger _logger;

        public GenericHandler(IEventSource eventSource, ILogger logger)
        {
            _eventSource = (EventSource)eventSource;
            _logger = logger;
        }
        
        public async Task Handle(IEvent message, IMessageHandlerContext context)
        {
            try
            {
                await _eventSource.OnNext(message);
            }
            catch (Exception e)
            {
                _logger.Debug(e, "Error in message handling");
                throw;
            }
        }
    }
}