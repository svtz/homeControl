using System.Text;
using homeControl.Domain.Events;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace homeControl.Interop.Rabbit
{
    [UsedImplicitly]
    internal sealed class JsonEventSerializer : IEventSerializer
    {
        private readonly Encoding _messageEncoding;
        private readonly JsonSerializerSettings _serializerSettings;

        public JsonEventSerializer(Encoding messageEncoding, JsonConverter[] converters)
        {
            Guard.DebugAssertArgumentNotNull(messageEncoding, nameof(messageEncoding));
            Guard.DebugAssertArgumentNotNull(converters, nameof(converters));

            _messageEncoding = messageEncoding;
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

            var messageString = _messageEncoding.GetString(messageBytes);
            var messageObject = JsonConvert.DeserializeObject<IEvent>(messageString, _serializerSettings);
            return messageObject;
        }

        public byte[] Serialize(IEvent message)
        {
            Guard.DebugAssertArgumentNotNull(message, nameof(message));

            var messageString = JsonConvert.SerializeObject(message, _serializerSettings);
            var messageBytes = _messageEncoding.GetBytes(messageString);
            return messageBytes;
        }
    }
}
