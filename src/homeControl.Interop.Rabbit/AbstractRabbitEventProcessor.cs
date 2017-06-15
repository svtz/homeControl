using System;
using RabbitMQ.Client;

namespace homeControl.Interop.Rabbit
{
    internal abstract class AbstractRabbitEventProcessor : IDisposable
    {
        protected string ExchangeName { get; }
        protected IEventSerializer EventSerializer { get; }
        protected IModel Channel { get; }

        private bool _disposed = false;

        protected AbstractRabbitEventProcessor(
            IConnection connection,
            IEventSerializer eventSerializer,
            string exchangeName,
            string exchangeType)
        {
            Guard.DebugAssertArgumentNotNull(connection, nameof(connection));
            Guard.DebugAssertArgumentNotNull(eventSerializer, nameof(eventSerializer));
            Guard.DebugAssertArgumentNotNull(exchangeName, nameof(exchangeName));
            Guard.DebugAssertArgumentNotNull(exchangeType, nameof(exchangeType));

            EventSerializer = eventSerializer;
            ExchangeName = exchangeName;
            Channel = CreateChannel(connection, exchangeName, exchangeType);
        }

        private static IModel CreateChannel(
            IConnection connection,
            string exchangeName,
            string exchangeType)
        {
            var channel = connection.CreateModel();
            channel.ExchangeDeclare(exchangeName, exchangeType);

            return channel;
        }

        protected void CheckNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        public void Dispose()
        {
            _disposed = true;
            Channel?.Dispose();
        }
    }
}