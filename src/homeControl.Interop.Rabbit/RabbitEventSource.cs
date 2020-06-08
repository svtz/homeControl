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
        private readonly ILogger _log;
        private readonly string _exchangeName;
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
            
            _log = log;
            _exchangeName = exchangeName;

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
                .Publish();
            _eventsConnection = _deserializedEvents.Connect();

            channel.BasicConsume(queue.QueueName, true, consumer);
        }

        public IObservable<TEvent> ReceiveEvents<TEvent>() where TEvent : IEvent
        {
            return _deserializedEvents.OfType<TEvent>()
                    .Do(msg => _log.Verbose("{ExchangeName}>>>{Event}", _exchangeName, msg));
        }

        public void Dispose()
        {
            _eventsConnection.Dispose();
        }
    }
}