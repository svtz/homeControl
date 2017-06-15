using System;
using System.Reactive.Linq;
using homeControl.Domain.Events;
using JetBrains.Annotations;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace homeControl.Interop.Rabbit
{
    [UsedImplicitly]
    internal sealed class RabbitEventSource : AbstractRabbitEventProcessor, IEventSource
    {
        private readonly EventingBasicConsumer _consumer;

        public RabbitEventSource(
            IConnection connection,
            IEventSerializer eventSerializer,
            string exchangeName,
            string exchangeType,
            string routingKey)
            : base(connection, eventSerializer, exchangeName, exchangeType)
        {
            _consumer = new EventingBasicConsumer(Channel);

            var queue = Channel.QueueDeclare();
            Channel.QueueBind(queue.QueueName, exchangeName, routingKey);
        }

        public IObservable<TEvent> GetMessages<TEvent>() where TEvent : IEvent
        {
            CheckNotDisposed();

            var messageSource = Observable.FromEventPattern<BasicDeliverEventArgs>(
                e => _consumer.Received += e,
                e => _consumer.Received -= e);

            return messageSource
                .Select(e => e.EventArgs.Body)
                .Select(EventSerializer.Deserialize)
                .OfType<TEvent>();
        }
    }
}