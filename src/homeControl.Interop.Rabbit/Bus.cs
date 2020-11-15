using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using homeControl.Domain.Events;
using JetBrains.Annotations;

namespace homeControl.Interop.Rabbit
{
    [UsedImplicitly]
    internal sealed class Bus : IEventSender, IEventReceiver, IDisposable
    {
        private readonly IEventProcessorFactory _factory;
        private readonly ExchangeConfiguration _routes;

        private readonly Dictionary<(string name, string type, string route), IEventReceiver> _allEventReceivers
            = new Dictionary<(string, string, string), IEventReceiver>();

        private readonly Dictionary<string, IEventSender> _allEventSenders =
            new Dictionary<string, IEventSender>();

        public Bus(IEventProcessorFactory factory, ExchangeConfiguration routes)
        {
            Guard.DebugAssertArgumentNotNull(routes, nameof(routes));
            Guard.DebugAssertArgumentNotNull(factory, nameof(factory));

            _factory = factory;
            _routes = routes;
        }

        void IEventSender.SendEvent(IEvent @event)
        {
            Guard.DebugAssertArgumentNotNull(@event, nameof(@event));
            CheckNotDisposed();
            EnsureInitialized();

            var suitableSenders =
                _routes.SendExchangesByType.Where(kv => kv.Key.IsInstanceOfType(@event))
                    .SelectMany(kv => kv.Value)
                    .Select(exchange => _allEventSenders[exchange])
                    .ToArray();

            if (!suitableSenders.Any())
                throw new InvalidOperationException($"No senders configured for event type \"{@event.GetType()}\".");

            foreach (var suitableSender in suitableSenders)
            {
                suitableSender.SendEvent(@event);
            }
        }

        IObservable<TEvent> IEventReceiver.ReceiveEvents<TEvent>()
        {
            CheckNotDisposed();
            EnsureInitialized();

            var suitableReceivers =
                _routes.ReceiveExchangesByType.Where(kv => kv.Key.IsAssignableFrom(typeof(TEvent)))
                    .SelectMany(kv => kv.Value)
                    .Select(exchange => _allEventReceivers[exchange])
                    .ToArray();

            if (!suitableReceivers.Any())
                throw new InvalidOperationException($"No receivers configured for event type \"{typeof(TEvent)}\".");

            return suitableReceivers.Aggregate(
                Observable.Empty<TEvent>(),
                (current, receiver) => current.Merge(receiver.ReceiveEvents<TEvent>()));
        }

        private bool _disposed = false;
        private void CheckNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            foreach (var sender in _allEventSenders)
            {
                (sender.Value as IDisposable)?.Dispose();
            }

            foreach (var receiver in _allEventReceivers)
            {
                (receiver.Value as IDisposable)?.Dispose();
            }

            _disposed = true;
        }

        private bool _initialized = false;
        private readonly object _initLock = new object();

        private void EnsureInitialized()
        {
            if (_initialized)
                return;

            lock (_initLock)
            {
                if (_initialized)
                    return;

                foreach (var exchangesByType in _routes.ReceiveExchangesByType)
                foreach (var exchange in exchangesByType.Value)
                {
                    if (!_allEventReceivers.ContainsKey(exchange))
                        _allEventReceivers[exchange] = CreateEventReceiver(exchange);
                }

                foreach (var exchangesByType in _routes.SendExchangesByType)
                foreach (var exchange in exchangesByType.Value)
                {
                    if (!_allEventSenders.ContainsKey(exchange))
                        _allEventSenders[exchange] = CreateEventSender(exchange);
                }

                _initialized = true;
            }
        }

        private IEventReceiver CreateEventReceiver((string name, string type, string route) newExchange)
        {
            return _factory.CreateReceiver(newExchange.name, newExchange.type, newExchange.route);
        }

        private IEventSender CreateEventSender(string exchangeName)
        {
            return _factory.CreateSender(exchangeName);
        }
    }
}
