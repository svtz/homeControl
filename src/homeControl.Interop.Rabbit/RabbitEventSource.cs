using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using homeControl.Domain.Events;
using JetBrains.Annotations;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace homeControl.Interop.Rabbit
{
    [UsedImplicitly]
    internal sealed class RabbitEventSource : IEventSource, IDisposable
    {
        private readonly IConnectableObservable<IEvent> _deserializedEvents;
        private readonly IDisposable _eventsConnection;

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

            channel.ExchangeDeclare(exchangeName, exchangeType);

            var queueName = $"{exchangeName}-{Guid.NewGuid()}";
            var queue = channel.QueueDeclare(queueName);
            channel.QueueBind(queue.QueueName, exchangeName, routingKey);

            var consumer = new EventingBasicConsumer(channel);

            var messageSource = Observable.FromEventPattern<BasicDeliverEventArgs>(
                e => consumer.Received += e,
                e => consumer.Received -= e);

            _deserializedEvents = messageSource
                .Select(e => e.EventArgs.Body)
                .Select(eventSerializer.Deserialize)
                .Do(msg => log.Verbose("{ExchangeName}>>>{Event}", exchangeName, msg))
                .Publish();
            _eventsConnection = _deserializedEvents.Connect();

            channel.BasicConsume(queue.QueueName, true, consumer);
        }

        public IObservable<TEvent> ReceiveEvents<TEvent>() where TEvent : IEvent
        {
            return _deserializedEvents.OfType<TEvent>();
        }

        public void Dispose()
        {
            _eventsConnection.Dispose();
        }
    }
}