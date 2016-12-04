using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace homeControl.Core
{
    public class Bus
    {
        private readonly IHandler[] _handlers;

        public Bus(params IHandler[] handlers)
        {
            _handlers = handlers;
        }

        public void PostMessage(IMessage message)
        {
            var suitableHandlers = _handlers.Where(h => h.CanHandle(message));
            foreach (var handler in suitableHandlers)
            {
                handler.Handle(message);
            }
        }
    }
}
