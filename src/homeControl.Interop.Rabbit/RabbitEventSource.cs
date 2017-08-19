using System;
using System.Reactive.Linq;
using homeControl.Domain.Events;
using JetBrains.Annotations;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace homeControl.Interop.Rabbit
{
    [UsedImplicitly]
    internal sealed class RabbitEventSource : IEventSource
    {
        private readonly IEventSerializer _eventSerializer;
        private readonly ILogger _log;
        private readonly string _exchangeName;
        private readonly EventingBasicConsumer _consumer;

        public RabbitEventSource(
            IModel channel,
            IEventSerializer eventSerializer,
            ILogger log,
            string exchangeName,
            string exchangeType,
            string routingKey)
        {
            Guard.DebugAssertArgumentNotNull(routingKey, nameof(routingKey));
            Guard.DebugAssertArgumentNotNull(exchangeName, nameof(exchangeName));
            Guard.DebugAssertArgumentNotNull(exchangeType, nameof(exchangeType));
            Guard.DebugAssertArgumentNotNull(eventSerializer, nameof(eventSerializer));
            Guard.DebugAssertArgumentNotNull(channel, nameof(channel));
            Guard.DebugAssertArgumentNotNull(log, nameof(log));

            _eventSerializer = eventSerializer;
            _log = log;
            _exchangeName = exchangeName;

            channel.ExchangeDeclare(exchangeName, exchangeType);

            var queueName = $"{exchangeName}-{Guid.NewGuid()}";
            var queue = channel.QueueDeclare(queueName);
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
                .Do(msg => _log.Verbose("{ExchangeName}>>>{Event}", _exchangeName, msg))
                .OfType<TEvent>();
        }
    }
}