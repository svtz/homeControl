using System.Threading;
using homeControl.Domain.Events;
using JetBrains.Annotations;
using RabbitMQ.Client;
using Serilog;

namespace homeControl.Interop.Rabbit
{
    [UsedImplicitly]
    internal sealed class RabbitEventProcessorFactory : IEventProcessorFactory
    {
        private readonly IModel _model;
        private readonly IEventSerializer _serializer;
        private readonly ILogger _logger;
        private readonly CancellationTokenSource _cts;

        public RabbitEventProcessorFactory(IModel model, IEventSerializer serializer, ILogger logger, CancellationTokenSource cts)
        {
            Guard.DebugAssertArgumentNotNull(model, nameof(model));
            Guard.DebugAssertArgumentNotNull(serializer, nameof(serializer));
            Guard.DebugAssertArgumentNotNull(logger, nameof(logger));
            Guard.DebugAssertArgumentNotNull(logger, nameof(cts));

            _model = model;
            _serializer = serializer;
            _logger = logger;
            _cts = cts;
        }

        public IEventReceiver CreateReceiver(string exchangeName, string exchangeType, string routingKey)
        {
            Guard.DebugAssertArgumentNotNull(exchangeName, nameof(exchangeName));
            Guard.DebugAssertArgumentNotNull(exchangeType, nameof(exchangeType));
            Guard.DebugAssertArgumentNotNull(routingKey, nameof(routingKey));

            return new RabbitEventReceiver(_model, _serializer, _logger.ForContext(typeof(RabbitEventReceiver)), exchangeName, exchangeType, routingKey);
        }

        public IEventSender CreateSender(string exchangeName)
        {
            Guard.DebugAssertArgumentNotNull(exchangeName, nameof(exchangeName));

            return new RabbitEventSender(_model, _serializer, _logger.ForContext(typeof(RabbitEventSender)), _cts, exchangeName);
        }
    }
}