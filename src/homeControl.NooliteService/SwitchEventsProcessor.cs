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
        private readonly IEventSource _source;
        private readonly ILogger _log;

        public SwitchEventsProcessor(ISwitchController switchController, IEventSource source, ILogger log)
        {
            Guard.DebugAssertArgumentNotNull(switchController, nameof(switchController));
            Guard.DebugAssertArgumentNotNull(source, nameof(source));
            Guard.DebugAssertArgumentNotNull(log, nameof(log));

            _switchController = switchController;
            _source = source;
            _log = log;
        }

        private readonly List<SwitchEventsObserver> _observers = new List<SwitchEventsObserver>();
        private readonly SemaphoreSlim _completionSemaphore = new SemaphoreSlim(0, 1);
        
        public void RunAsync(CancellationToken ct)
        {
            var eventSource = _source.ReceiveEvents<AbstractSwitchEvent>();
            eventSource
                .GroupBy(e => e.SwitchId)
                .ForEachAsync(switchObservable =>
                {
                    var observer = new SwitchEventsObserver(_switchController, _log, ct);
                    _observers.Add(observer);
                    switchObservable.Subscribe(observer);
                }, ct)
                .ContinueWith(t => _completionSemaphore.Release(), ct);
        }

        public async Task Completion(CancellationToken ct)
        {
            await _completionSemaphore.WaitAsync(ct);
            await Task.WhenAll(_observers.Select(o => o.Completion(ct)));
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
