namespace homeControl.Core
{
    public interface IEventPublisher
    {
        void PostMessage(IMessage message);
    }
}