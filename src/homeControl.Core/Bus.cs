using System;
using System.Collections.Concurrent;
using System.Linq;

namespace homeControl.Core
{
    public class Bus
    {
        private readonly IHandler[] _handlers;
        private readonly ConcurrentQueue<IMessage> _queue;


        public Bus(params IHandler[] handlers)
        {
            _handlers = handlers;
            _queue = new ConcurrentQueue<IMessage>();
        }

        public void PostMessage(IMessage message)
        {
            _queue.Enqueue(message);
        }

        public void ProcessMessages()
        {
            IMessage message;
            while (_queue.TryDequeue(out message))
            {
                ProcessMessage(message);
            }
        }

        private void ProcessMessage(IMessage message)
        {
            var suitableHandlers = _handlers.Where(h => h.CanHandle(message));
            foreach (var handler in suitableHandlers)
            {
                handler.Handle(message);
            }
        }
    }
}
