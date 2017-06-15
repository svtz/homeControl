using System.Reactive.Linq;
using homeControl.Domain.Events;
using homeControl.Events.System;
using JetBrains.Annotations;

namespace homeControl.ConfigurationStore
{
    [UsedImplicitly]
    internal sealed class ConfigurationRequestsProcessor
    {
        private readonly IEventSource _eventSource;
        private readonly IEventSender _eventSender;
        private readonly ConfigurationProvider _configurationProvider;

        public ConfigurationRequestsProcessor(
            IEventSource eventSource, 
            IEventSender eventSender,
            ConfigurationProvider configurationProvider)
        {
            Guard.DebugAssertArgumentNotNull(eventSource, nameof(eventSource));
            Guard.DebugAssertArgumentNotNull(eventSender, nameof(eventSender));
            Guard.DebugAssertArgumentNotNull(configurationProvider, nameof(configurationProvider));

            _eventSource = eventSource;
            _eventSender = eventSender;
            _configurationProvider = configurationProvider;
        }

        public void Run()
        {
            _eventSource
                .GetMessages<ConfigurationRequestEvent>()
                .ForEachAsync(ProcessRequest)
                .Wait();
        }

        private void ProcessRequest(ConfigurationRequestEvent request)
        {
            Guard.DebugAssertArgumentNotNull(request, nameof(request));

            if (string.IsNullOrEmpty(request.ConfigurationKey))
            {
                return;
            }

            var key = request.ConfigurationKey;
            var configuration = _configurationProvider.GetConfiguration(key);
            if (string.IsNullOrEmpty(configuration))
            {
                return;
            }

            var reply = new ConfigurationResponseEvent
            {
                Configuration = configuration,
                Address = request.ReplyAddress
            };

            _eventSender.SendEvent(reply);
        }
    }
}