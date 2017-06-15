using homeControl.Domain.Events;
using JetBrains.Annotations;
using RabbitMQ.Client;

namespace homeControl.Interop.Rabbit
{
    [UsedImplicitly]
    internal sealed class RabbitEventSender : AbstractRabbitEventProcessor, IEventSender
    {
        public RabbitEventSender(
            IConnection connection,
            IEventSerializer eventSerializer,
            string exchangeName,
            string exchangeType)
            : base(connection, eventSerializer, exchangeName, exchangeType)
        {
        }

        public void SendEvent(IEvent @event)
        {
            Guard.DebugAssertArgumentNotNull(@event, nameof(@event));
            CheckNotDisposed();

            var address = (@event as IEventWithAddress)?.Address;
            var messageBytes = EventSerializer.Serialize(@event);

            Channel.BasicPublish(ExchangeName, address, false, null, messageBytes);
        }
    }
}