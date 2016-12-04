using System;
using System.Collections.Concurrent;
using System.Linq;

namespace homeControl.Core
{
    internal class Bus : IEventPublisher, IEventProcessor
    {
        private readonly IHandler[] _handlers;
        private readonly ConcurrentQueue<IEvent> _queue;


        public Bus(params IHandler[] handlers)
        {
            _handlers = handlers;
            _queue = new ConcurrentQueue<IEvent>();
        }

        public void PublishEvent(IEvent @event)
        {
            _queue.Enqueue(@event);
        }

        public void ProcessEvents()
        {
            IEvent @event;
            while (_queue.TryDequeue(out @event))
            {
                ProcessEvent(@event);
            }
        }

        private void ProcessEvent(IEvent @event)
        {
            var suitableHandlers = _handlers.Where(h => h.CanHandle(@event));
            foreach (var handler in suitableHandlers)
            {
                handler.Handle(@event);
            }
        }
    }
}
