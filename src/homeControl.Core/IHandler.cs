using System;

namespace homeControl.Core
{
    public interface IHandler
    {
        bool CanHandle(IEvent @event);
        void Handle(IEvent @event);
    }
}
