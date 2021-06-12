using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Configuration;
using JetBrains.Annotations;
using Serilog;

namespace homeControl.ConfigurationStore
{
    [UsedImplicitly]
    internal sealed class ConfigurationRequestsProcessor
    {
        private readonly IEventReceiver _eventReceiver;
        private readonly IEventSender _eventSender;
        private readonly ConfigurationProvider _configurationProvider;
        private readonly ILogger _log;

        public ConfigurationRequestsProcessor(
            IEventReceiver eventReceiver, 
            IEventSender eventSender,
            ConfigurationProvider configurationProvider,
            ILogger log)
        {
            Guard.DebugAssertArgumentNotNull(eventReceiver, nameof(eventReceiver));
            Guard.DebugAssertArgumentNotNull(eventSender, nameof(eventSender));
            Guard.DebugAssertArgumentNotNull(configurationProvider, nameof(configurationProvider));

            _eventReceiver = eventReceiver;
            _eventSender = eventSender;
            _configurationProvider = configurationProvider;
            _log = log;
        }

        public Task Run(CancellationToken ct)
        {
            _log.Debug("Starting configuration request processor.");
            return _eventReceiver
                .ReceiveEvents<ConfigurationRequestEvent>()
                .ForEachAsync(ProcessRequest, ct);
        }

        private void ProcessRequest(ConfigurationRequestEvent request)
        {
            Guard.DebugAssertArgumentNotNull(request, nameof(request));

            _log.Debug("Processing configuration request: {ConfigurationKey}, {ReplyAddress}", request.ConfigurationKey, request.ReplyAddress);

            if (string.IsNullOrEmpty(request.ConfigurationKey))
            {
                _log.Debug("Invalid configuration key.");
                return;
            }

            var key = request.ConfigurationKey;
            var configuration = _configurationProvider.GetConfiguration(key);
            if (string.IsNullOrEmpty(configuration))
            {
                _log.Warning("No configuration found for {ConfigurationKey}, requester: {ReplyAddress}", request.ConfigurationKey, request.ReplyAddress);
                return;
            }

            var reply = new ConfigurationResponseEvent
            {
                Configuration = configuration,
                Address = request.ReplyAddress
            };

            _eventSender.SendEvent(reply);
            _log.Debug("Configuration request processed.");
        }
    }
}