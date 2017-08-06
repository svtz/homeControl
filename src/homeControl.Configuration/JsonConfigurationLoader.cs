using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using homeControl.Domain.Events;
using homeControl.Events.System;
using Newtonsoft.Json;
using Serilog;

namespace homeControl.Configuration
{
    internal sealed class JsonConfigurationLoader<TConfiguration> : IConfigurationLoader<TConfiguration>
    {
        private readonly JsonConverter[] _converters;
        private readonly IEventSender _sender;
        private readonly IEventSource _source;
        private readonly string _serviceName;
        private readonly ILogger _log;

        public JsonConfigurationLoader(
            JsonConverter[] converters,
            IEventSender sender,
            IEventSource source,
            string serviceName,
            ILogger log)
        {
            Guard.DebugAssertArgumentNotNull(converters, nameof(converters));
            Guard.DebugAssertArgumentNotNull(sender, nameof(sender));
            Guard.DebugAssertArgumentNotNull(source, nameof(source));
            Guard.DebugAssertArgumentNotNull(serviceName, nameof(serviceName));

            _converters = converters;
            _sender = sender;
            _source = source;
            _serviceName = serviceName;
            _log = log;
        }

        public async Task<TConfiguration> Load(string configKey)
        {
            Guard.DebugAssertArgumentNotNull(configKey, nameof(configKey));

            _log.Debug("Loading configuration {ConfigurationKey}", configKey);
            
            var request = new ConfigurationRequestEvent()
            {
                ConfigurationKey = configKey,
                ReplyAddress = _serviceName
            };
            var response = _source.ReceiveEvents<ConfigurationResponseEvent>().FirstAsync();
            _sender.SendEvent(request);
            
            var config = (await response).Configuration;

            _log.Verbose("Got response, deserializing...");

            var serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = _converters
            };

            try
            {
                var result = JsonConvert.DeserializeObject<TConfiguration>(config, serializerSettings);
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
