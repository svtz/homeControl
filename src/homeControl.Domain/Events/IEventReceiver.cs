using System;

namespace homeControl.Domain.Events
{
    public interface IEventReceiver
    {
        IObservable<TEvent> ReceiveEvents<TEvent>() where TEvent : IEvent;
    }
}