using System;
using System.Reactive.Linq;
using homeControl.Domain.Events;
using JetBrains.Annotations;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace homeControl.Interop.Rabbit
{
    [UsedImplicitly]
    internal sealed class RabbitEventSource : IEventSource
    {
        private readonly IEventSerializer _eventSerializer;
        private readonly EventingBasicConsumer _consumer;

        public RabbitEventSource(
            IModel channel,
            IEventSerializer eventSerializer,
            string exchangeName,
            string exchangeType,
            string routingKey)
        {
            Guard.DebugAssertArgumentNotNull(routingKey, nameof(routingKey));
            Guard.DebugAssertArgumentNotNull(exchangeName, nameof(exchangeName));
            Guard.DebugAssertArgumentNotNull(exchangeType, nameof(exchangeType));
            Guard.DebugAssertArgumentNotNull(eventSerializer, nameof(eventSerializer));
            Guard.DebugAssertArgumentNotNull(channel, nameof(channel));

            _eventSerializer = eventSerializer;

            channel.ExchangeDeclare(exchangeName, exchangeType);

            var queue = channel.QueueDeclare();
            channel.QueueBind(queue.QueueName, exchangeName, routingKey);

            _consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(queue.QueueName, true, _consumer);
        }

        public IObservable<TEvent> ReceiveEvents<TEvent>() where TEvent : IEvent
        {
            var messageSource = Observable.FromEventPattern<BasicDeliverEventArgs>(
                e => _consumer.Received += e,
                e => _consumer.Received -= e);

            return messageSource
                .Select(e => e.EventArgs.Body)
                .Select(_eventSerializer.Deserialize)
                .OfType<TEvent>();
        }
    }
}