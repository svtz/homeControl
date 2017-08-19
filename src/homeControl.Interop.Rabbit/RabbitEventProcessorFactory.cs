using homeControl.Domain.Events;
using JetBrains.Annotations;
using RabbitMQ.Client;

namespace homeControl.Interop.Rabbit
{
    [UsedImplicitly]
    internal sealed class RabbitEventProcessorFactory : IEventProcessorFactory
    {
        private readonly IModel _model;
        private readonly IEventSerializer _serializer;

        public RabbitEventProcessorFactory(IModel model, IEventSerializer serializer)
        {
            Guard.DebugAssertArgumentNotNull(model, nameof(model));
            Guard.DebugAssertArgumentNotNull(serializer, nameof(serializer));

            _model = model;
            _serializer = serializer;
        }

        public IEventSource CreateSource(string exchangeName, string exchangeType, string routingKey)
        {
            Guard.DebugAssertArgumentNotNull(exchangeName, nameof(exchangeName));
            Guard.DebugAssertArgumentNotNull(exchangeType, nameof(exchangeType));
            Guard.DebugAssertArgumentNotNull(routingKey, nameof(routingKey));

            return new RabbitEventSource(_model, _serializer, exchangeName, exchangeType, routingKey);
        }

        public IEventSender CreateSender(string exchangeName)
        {
            Guard.DebugAssertArgumentNotNull(exchangeName, nameof(exchangeName));

            return new RabbitEventSender(_model, _serializer, exchangeName);
        }
    }
}