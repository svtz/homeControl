namespace homeControl.Core
{
    public interface IEventProcessor
    {
        EventProcessingResult ProcessEvents();
    }
}