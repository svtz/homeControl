namespace homeControl.Core
{
    public class EventProcessingLoop
    {
        private readonly IEventProcessor _eventProcessor;

        public EventProcessingLoop(IEventProcessor processor)
        {
            _eventProcessor = processor;
        }

        void Run()
        {
            while (true)
            {
                _eventProcessor.ProcessEvents();
            }
        }
    }
}