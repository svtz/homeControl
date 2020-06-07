namespace homeControl.Domain.Events
{
    public interface IEventSender
    {
        void SendEvent(IEvent @event);
    }
}