using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using homeControl.Domain.Events;
using JetBrains.Annotations;

namespace homeControl.Interop.Rabbit
{
    [UsedImplicitly]
    internal sealed class ExchangeConfiguration
    {
        private readonly Dictionary<Type, List<(string, string, string)>> _sourceExchangesByType 
            = new Dictionary<Type, List<(string, string, string)>>();
        public IReadOnlyDictionary<Type, List<(string name, string type, string route)>> SourceExchangesByType
            => new ReadOnlyDictionary<Type, List<(string, string, string)>>(_sourceExchangesByType);

        private readonly Dictionary<Type, List<(string, string)>> _sendExchangesByType
            = new Dictionary<Type, List<(string, string)>>();
        public IReadOnlyDictionary<Type, List<(string name, string type)>> SendExchangesByType
            => new ReadOnlyDictionary<Type, List<(string, string)>>(_sendExchangesByType);

        public void ConfigureEventSender(Type eventType, string exchangeName, string exchangeType)
        {
            Guard.DebugAssertArgumentNotNull(eventType, nameof(eventType));
            Guard.DebugAssertArgument(typeof(IEvent).IsAssignableFrom(eventType), nameof(eventType));
            Guard.DebugAssertArgumentNotNull(exchangeName, nameof(exchangeName));
            Guard.DebugAssertArgumentNotNull(exchangeType, nameof(exchangeType));
            Guard.DebugAssertArgumentNotNull(exchangeType, nameof(exchangeType));

            if (!_sendExchangesByType.TryGetValue(eventType, out var exchangesByType))
            {
                exchangesByType = new List<(string, string)>();
                _sendExchangesByType.Add(eventType, exchangesByType);
            }

            exchangesByType.Add((exchangeName, exchangeType));
        }

        public void ConfigureEventSource(Type eventType, string exchangeName, string exchangeType, string routingKey)
        {
            Guard.DebugAssertArgumentNotNull(eventType, nameof(eventType));
            Guard.DebugAssertArgument(typeof(IEvent).IsAssignableFrom(eventType), nameof(eventType));
            Guard.DebugAssertArgumentNotNull(exchangeName, nameof(exchangeName));
            Guard.DebugAssertArgumentNotNull(exchangeType, nameof(exchangeType));

            if (!_sourceExchangesByType.TryGetValue(eventType, out var exchangesByType))
            {
                exchangesByType = new List<(string, string, string)>();
                _sourceExchangesByType.Add(eventType, exchangesByType);
            }

            exchangesByType.Add((exchangeName, exchangeType, routingKey));
        }
    }
}