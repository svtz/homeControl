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
        private readonly Dictionary<Type, List<(string, string, string)>> _receiveExchangesByType 
            = new Dictionary<Type, List<(string, string, string)>>();
        public IReadOnlyDictionary<Type, List<(string name, string type, string route)>> ReceiveExchangesByType
            => new ReadOnlyDictionary<Type, List<(string, string, string)>>(_receiveExchangesByType);

        private readonly Dictionary<Type, List<string>> _sendExchangesByType
            = new Dictionary<Type, List<string>>();
        public IReadOnlyDictionary<Type, List<string>> SendExchangesByType
            => new ReadOnlyDictionary<Type, List<string>>(_sendExchangesByType);

        public void ConfigureEventSender(Type eventType, string exchangeName)
        {
            Guard.DebugAssertArgumentNotNull(eventType, nameof(eventType));
            Guard.DebugAssertArgument(typeof(IEvent).IsAssignableFrom(eventType), nameof(eventType));
            Guard.DebugAssertArgumentNotNull(exchangeName, nameof(exchangeName));

            if (!_sendExchangesByType.TryGetValue(eventType, out var exchangesByType))
            {
                exchangesByType = new List<string>();
                _sendExchangesByType.Add(eventType, exchangesByType);
            }

            exchangesByType.Add((exchangeName));
        }

        public void ConfigureEventReceiver(Type eventType, string exchangeName, string exchangeType, string routingKey)
        {
            Guard.DebugAssertArgumentNotNull(eventType, nameof(eventType));
            Guard.DebugAssertArgument(typeof(IEvent).IsAssignableFrom(eventType), nameof(eventType));
            Guard.DebugAssertArgumentNotNull(exchangeName, nameof(exchangeName));
            Guard.DebugAssertArgumentNotNull(exchangeType, nameof(exchangeType));

            if (!_receiveExchangesByType.TryGetValue(eventType, out var exchangesByType))
            {
                exchangesByType = new List<(string, string, string)>();
                _receiveExchangesByType.Add(eventType, exchangesByType);
            }

            exchangesByType.Add((exchangeName, exchangeType, routingKey));
        }
    }
}