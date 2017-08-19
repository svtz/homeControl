using System;
using System.Text;
using homeControl.Domain.Events;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Serilog;

namespace homeControl.Interop.Rabbit
{
    [UsedImplicitly]
    internal sealed class JsonEventSerializer : IEventSerializer
    {
        private readonly Encoding _messageEncoding;
        private readonly ILogger _log;
        private readonly JsonSerializerSettings _serializerSettings;

        public JsonEventSerializer(Encoding messageEncoding, JsonConverter[] converters, ILogger log)
        {
            Guard.DebugAssertArgumentNotNull(messageEncoding, nameof(messageEncoding));
            Guard.DebugAssertArgumentNotNull(converters, nameof(converters));
            Guard.DebugAssertArgumentNotNull(log, nameof(log));

            _messageEncoding = messageEncoding;
            _log = log;
            _serializerSettings = new JsonSerializerSettings
            {
                Converters = converters,
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.All
            };
        }

        public IEvent Deserialize(byte[] messageBytes)
        {
            Guard.DebugAssertArgumentNotNull(messageBytes, nameof(messageBytes));

            try
            {
                var messageString = _messageEncoding.GetString(messageBytes);
                var messageObject = JsonConvert.DeserializeObject<IEvent>(messageString, _serializerSettings);
                return messageObject;
            }
            catch (Exception e)
            {
                _log.Error("Error while deserializing message: {Exception}", e);
                throw;
            }
        }

        public byte[] Serialize(IEvent message)
        {
            Guard.DebugAssertArgumentNotNull(message, nameof(message));

            try
            {
                var messageString = JsonConvert.SerializeObject(message, _serializerSettings);
                var messageBytes = _messageEncoding.GetBytes(messageString);
                return messageBytes;
            }
            catch (Exception e)
            {
                _log.Error("Error while serializing message: {Exception}", e);
                throw;
            }
        }
    }
}
