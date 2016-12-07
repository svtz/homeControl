using System;
using System.Diagnostics;
using System.Threading;
using homeControl.Core.Misc;

namespace homeControl.Core
{
    public class EventProcessingLoop
    {
        private readonly IEventProcessor _eventProcessor;

        public TimeSpan ThrottleTime { get; set; } = TimeSpan.Zero;

        public EventProcessingLoop(IEventProcessor processor)
        {
            Guard.DebugAssertArgumentNotNull(processor, nameof(processor));

            _eventProcessor = processor;
        }

        public void Run(CancellationToken token)
        {
            Guard.DebugAssertArgumentNotDefault(token, nameof(token));

            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                var processingResult = _eventProcessor.ProcessEvents();

                if (processingResult == EventProcessingResult.Idle 
                    && ThrottleTime > TimeSpan.Zero)
                {
                    Thread.Sleep(ThrottleTime);
                }
            }
        }
    }
}