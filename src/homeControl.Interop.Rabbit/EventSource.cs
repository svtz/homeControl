using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading.Tasks;
using homeControl.Domain.Events;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using IEvent = homeControl.Domain.Events.IEvent;

namespace homeControl.Interop.Rabbit
{
    [UsedImplicitly]
    internal sealed class EventSource : IEventSource
    {
        private readonly ILogger _logger;
        private readonly ISubject<IEvent> _events = new Subject<IEvent>();

        public EventSource(ILogger logger)
        {
            _logger = logger;
        }

        public IObservable<TEvent> ReceiveEvents<TEvent>() where TEvent : IEvent
        {
            return _events.OfType<TEvent>()
                .Do(LogMessage);
        }

        private void LogMessage<TEvent>(TEvent message) where TEvent : IEvent
        {
            if (_logger.IsEnabled(LogEventLevel.Verbose))
            {
                _logger.Verbose("<<< {messageType}: {message}",
                    message.GetType(),
                    JsonConvert.SerializeObject(message, Formatting.Indented));
            }
        }

        internal Task OnNext(IEvent message)
        {
            _events.OnNext(message);
            return Task.CompletedTask;
        }
    }
}
