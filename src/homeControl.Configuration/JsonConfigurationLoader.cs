using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Configuration;
using Newtonsoft.Json;
using Serilog;

namespace homeControl.Configuration
{
    internal sealed class JsonConfigurationLoader<TConfiguration> : IConfigurationLoader<TConfiguration>
    {
        private readonly JsonConverter[] _converters;
        private readonly IEventSender _sender;
        private readonly IEventReceiver _receiver;
        private readonly ServiceNameHolder _serviceName;
        private readonly ILogger _log;

        public JsonConfigurationLoader(
            IEnumerable<JsonConverter> converters,
            IEventSender sender,
            IEventReceiver receiver,
            ServiceNameHolder serviceName,
            ILogger log)
        {
            Guard.DebugAssertArgumentNotNull(converters, nameof(converters));
            Guard.DebugAssertArgumentNotNull(sender, nameof(sender));
            Guard.DebugAssertArgumentNotNull(receiver, nameof(receiver));
            Guard.DebugAssertArgumentNotNull(serviceName, nameof(serviceName));

            _converters = converters.ToArray();
            _sender = sender;
            _receiver = receiver;
            _serviceName = serviceName;
            _log = log;
        }

        private async Task<string> WaitResponse(IObservable<ConfigurationResponseEvent> responseObservable)
        {
            Guard.DebugAssertArgumentNotNull(responseObservable, nameof(responseObservable));
            return (await responseObservable).Configuration;
        }
        
        public async Task<TConfiguration> Load(string configKey)
        {
            Guard.DebugAssertArgumentNotNull(configKey, nameof(configKey));

            _log.Debug("Loading configuration {ConfigurationKey}", configKey);
            
            var request = new ConfigurationRequestEvent()
            {
                ConfigurationKey = configKey,
                ReplyAddress = _serviceName.ServiceName
            };
            var response = _receiver.ReceiveEvents<ConfigurationResponseEvent>().FirstAsync();
            _sender.SendEvent(request);

            var configTask = WaitResponse(response);
            await Task.WhenAny(
                configTask,
                Task.Delay(TimeSpan.FromSeconds(10)));

            if (!configTask.IsCompleted)
            {
                throw new TimeoutException("Unable to get configuration response.");
            }

            _log.Verbose("Got response, deserializing...");

            var serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = _converters
            };

            try
            {
                var result = JsonConvert.DeserializeObject<TConfiguration>(configTask.Result, serializerSettings);
                _log.Debug("Configuration {ConfigurationKey} loaded successfully.", configKey);
                return result;
            }
            catch (JsonException ex)
            {
                throw new InvalidConfigurationException(ex, $"Error reading json from response.");
            }
        }
    }
}
