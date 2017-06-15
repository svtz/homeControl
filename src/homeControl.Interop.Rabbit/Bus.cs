using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using homeControl.Domain.Events;
using JetBrains.Annotations;

namespace homeControl.Interop.Rabbit
{
    internal interface IEventProcessorFactory
    {
        IEventSource CreateSource(string exchangeName, string exchangeType, string routingKey);
        IEventSender CreateSender(string exchangeName, string exchangeType);
    }

    [UsedImplicitly]
    internal sealed class Bus : IEventSender, IEventSource, IDisposable
    {
        private readonly IEventProcessorFactory _factory;
        private readonly ExchangeConfiguration _routes;

        private readonly Dictionary<(string name, string type, string route), IEventSource> _allEventSources
            = new Dictionary<(string, string, string), IEventSource>();

        private readonly Dictionary<(string name, string type), IEventSender> _allEventSenders =
            new Dictionary<(string, string), IEventSender>();

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

        IObservable<TEvent> IEventSource.GetMessages<TEvent>()
        {
            CheckNotDisposed();
            EnsureInitialized();

            var suitableSources =
                _routes.SourceExchangesByType.Where(kv => kv.Key.IsAssignableFrom(typeof(TEvent)))
                    .SelectMany(kv => kv.Value)
                    .Select(exchange => _allEventSources[exchange])
                    .ToArray();

            if (!suitableSources.Any())
                throw new InvalidOperationException($"No sources configured for event type \"{typeof(TEvent)}\".");

            return suitableSources.Aggregate(
                Observable.Empty<TEvent>(),
                (current, suitableSource) => current.Merge(suitableSource.GetMessages<TEvent>()));
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

            foreach (var source in _allEventSources)
            {
                (source.Value as IDisposable)?.Dispose();
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

                foreach (var exchangesByType in _routes.SourceExchangesByType)
                foreach (var exchange in exchangesByType.Value)
                {
                    if (!_allEventSources.ContainsKey(exchange))
                        _allEventSources[exchange] = CreateEventSource(exchange);
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

        private IEventSource CreateEventSource((string name, string type, string route) newExchange)
        {
            return _factory.CreateSource(newExchange.name, newExchange.type, newExchange.route);
        }

        private IEventSender CreateEventSender((string name, string type) newExchange)
        {
            return _factory.CreateSender(newExchange.name, newExchange.type);
        }
    }
}
