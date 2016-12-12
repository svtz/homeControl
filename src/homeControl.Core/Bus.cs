using System.Collections.Concurrent;
using System.Linq;

namespace homeControl.Core
{
    internal class Bus : IEventPublisher, IEventProcessor
    {
        private readonly IHandlerRepository _handlerRepository;
        private readonly ConcurrentQueue<IEvent> _queue;


        public Bus(IHandlerRepository handlerRepository)
        {
            _handlerRepository = handlerRepository;
            Guard.DebugAssertArgumentNotNull(handlerRepository, nameof(handlerRepository));

            _queue = new ConcurrentQueue<IEvent>();
        }

        public void PublishEvent(IEvent @event)
        {
            Guard.DebugAssertArgumentNotNull(@event, nameof(@event));

            _queue.Enqueue(@event);
        }

        public EventProcessingResult ProcessEvents()
        {
            var eventsProcessed = false;

            IEvent @event;
            while (_queue.TryDequeue(out @event))
            {
                ProcessEvent(@event);
                eventsProcessed = true;
            }

            return eventsProcessed ? EventProcessingResult.Complete : EventProcessingResult.Idle;
        }

        private void ProcessEvent(IEvent @event)
        {
            var suitableHandlers = _handlerRepository.GetHandlers().Where(h => h.CanHandle(@event));
            foreach (var handler in suitableHandlers)
            {
                handler.Handle(@event);
            }
        }
    }
}
