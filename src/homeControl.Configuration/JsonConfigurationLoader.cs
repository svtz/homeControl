using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using homeControl.Domain.Events;
using homeControl.Events.System;
using Newtonsoft.Json;

namespace homeControl.Configuration
{
    internal sealed class JsonConfigurationLoader<TConfiguration> : IConfigurationLoader<TConfiguration>
    {
        private readonly JsonConverter[] _converters;
        private readonly IEventSender _sender;
        private readonly IEventSource _source;
        private readonly string _serviceName;

        public JsonConfigurationLoader(
            JsonConverter[] converters,
            IEventSender sender,
            IEventSource source,
            string serviceName)
        {
            Guard.DebugAssertArgumentNotNull(converters, nameof(converters));
            Guard.DebugAssertArgumentNotNull(sender, nameof(sender));
            Guard.DebugAssertArgumentNotNull(source, nameof(source));
            Guard.DebugAssertArgumentNotNull(serviceName, nameof(serviceName));

            _converters = converters;
            _sender = sender;
            _source = source;
            _serviceName = serviceName;
        }

        public async Task<TConfiguration> Load(string configKey)
        {
            Guard.DebugAssertArgumentNotNull(configKey, nameof(configKey));

            var request = new ConfigurationRequestEvent()
            {
                ConfigurationKey = configKey,
                ReplyAddress = _serviceName
            };
            var response = _source.ReceiveEvents<ConfigurationResponseEvent>().FirstAsync();
            _sender.SendEvent(request);
            

            var config = (await response).Configuration;
            var serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = _converters
            };

            try
            {
                return JsonConvert.DeserializeObject<TConfiguration>(config, serializerSettings);
            }
            catch (JsonException ex)
            {
                throw new InvalidConfigurationException(ex, $"Error reading json from response.");
            }
        }
    }
}
