using System;

namespace homeControl.Domain.Events
{
    public interface IEventSource
    {
        IObservable<TEvent> GetMessages<TEvent>() where TEvent : IEvent;
    }
}