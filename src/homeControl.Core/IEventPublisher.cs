namespace homeControl.Core
{
    public interface IEventPublisher
    {
        void PublishEvent(IEvent @event);
    }
}