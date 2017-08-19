using System;
using homeControl.Domain.Events;
using JetBrains.Annotations;
using RabbitMQ.Client;
using Serilog;

namespace homeControl.Interop.Rabbit
{
    [UsedImplicitly]
    internal sealed class RabbitEventSender : IEventSender
    {
        private readonly IModel _channel;
        private readonly IEventSerializer _eventSerializer;
        private readonly ILogger _log;
        private readonly string _exchangeName;

        public RabbitEventSender(
            IModel channel,
            IEventSerializer eventSerializer,
            ILogger log,
            string exchangeName)
        {
            Guard.DebugAssertArgumentNotNull(channel, nameof(channel));
            Guard.DebugAssertArgumentNotNull(eventSerializer, nameof(eventSerializer));
            Guard.DebugAssertArgumentNotNull(exchangeName, nameof(exchangeName));
            Guard.DebugAssertArgumentNotNull(log, nameof(log));

            _channel = channel;
            _eventSerializer = eventSerializer;
            _log = log;
            _exchangeName = exchangeName;
        }

        public void SendEvent(IEvent @event)
        {
            Guard.DebugAssertArgumentNotNull(@event, nameof(@event));

            var address = (@event as IEventWithAddress)?.Address ?? string.Empty;
            var messageBytes = _eventSerializer.Serialize(@event);

            _channel.BasicPublish(_exchangeName, address, false, null, messageBytes);
            _log.Verbose("{ExchangeName}<<<{Event}", _exchangeName, @event);
        }
    }
}