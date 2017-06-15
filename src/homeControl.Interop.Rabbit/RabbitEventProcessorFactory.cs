using homeControl.Domain.Events;
using JetBrains.Annotations;
using RabbitMQ.Client;

namespace homeControl.Interop.Rabbit
{
    [UsedImplicitly]
    internal sealed class RabbitEventProcessorFactory : IEventProcessorFactory
    {
        private readonly IConnection _connection;
        private readonly IEventSerializer _serializer;

        public RabbitEventProcessorFactory(IConnection connection, IEventSerializer serializer)
        {
            Guard.DebugAssertArgumentNotNull(connection, nameof(connection));
            Guard.DebugAssertArgumentNotNull(serializer, nameof(serializer));

            _connection = connection;
            _serializer = serializer;
        }

        public IEventSource CreateSource(string exchangeName, string exchangeType, string routingKey)
        {
            Guard.DebugAssertArgumentNotNull(exchangeName, nameof(exchangeName));
            Guard.DebugAssertArgumentNotNull(exchangeType, nameof(exchangeType));
            Guard.DebugAssertArgumentNotNull(routingKey, nameof(routingKey));

            return new RabbitEventSource(_connection, _serializer, exchangeName, exchangeType, routingKey);
        }

        public IEventSender CreateSender(string exchangeName, string exchangeType)
        {
            Guard.DebugAssertArgumentNotNull(exchangeName, nameof(exchangeName));
            Guard.DebugAssertArgumentNotNull(exchangeType, nameof(exchangeType));

            return new RabbitEventSender(_connection, _serializer, exchangeName, exchangeType);
        }
    }
}