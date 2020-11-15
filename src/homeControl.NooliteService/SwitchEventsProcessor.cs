using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Switches;
using homeControl.NooliteService.SwitchController;
using JetBrains.Annotations;
using Serilog;

namespace homeControl.NooliteService
{
    [UsedImplicitly]
    internal sealed class SwitchEventsProcessor : IDisposable
    {
        private readonly ISwitchController _switchController;
        private readonly IEventReceiver _receiver;
        private readonly ILogger _log;

        public SwitchEventsProcessor(ISwitchController switchController, IEventReceiver receiver, ILogger log)
        {
            Guard.DebugAssertArgumentNotNull(switchController, nameof(switchController));
            Guard.DebugAssertArgumentNotNull(receiver, nameof(receiver));
            Guard.DebugAssertArgumentNotNull(log, nameof(log));

            _switchController = switchController;
            _receiver = receiver;
            _log = log;
        }

        private readonly List<SwitchEventsObserver> _observers = new List<SwitchEventsObserver>();
        private readonly SemaphoreSlim _completionSemaphore = new SemaphoreSlim(0, 1);
        
        public void Start(CancellationToken ct)
        {
            _log.Debug("Starting events processing.");
            var eventReceiver = _receiver.ReceiveEvents<AbstractSwitchEvent>();
            eventReceiver
                .GroupBy(e => e.SwitchId)
                .ForEachAsync(switchObservable =>
                {
                    _log.Debug("Received new SwitchId={SwitchId}, creating observer.", switchObservable.Key);
                    var observer = new SwitchEventsObserver(_switchController, _log.ForContext<SwitchEventsObserver>(), ct);
                    _observers.Add(observer);
                    switchObservable.Subscribe(observer);
                }, ct)
                .ContinueWith(t => _completionSemaphore.Release(), ct);
        }

        public async Task Completion(CancellationToken ct)
        {
            _log.Debug("Awaiting completion.");
            await _completionSemaphore.WaitAsync(ct);
            await Task.WhenAll(_observers.Select(o => o.Completion(ct)));
            _log.Debug("Complete!");
        }

        public void Dispose()
        {
            _completionSemaphore?.Dispose();
            foreach (var observer in _observers)
            {
                observer.Dispose();
            }
        }
    }
}
