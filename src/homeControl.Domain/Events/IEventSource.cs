using System;

namespace homeControl.Domain.Events
{
    public interface IEventSource
    {
        IObservable<TEvent> ReceiveEvents<TEvent>() where TEvent : IEvent;
    }
}