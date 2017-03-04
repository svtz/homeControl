using System;
using Newtonsoft.Json;

namespace homeControl.Configuration.Sensors
{
    internal sealed class SensorIdSerializer : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var sensorId = (SensorId)value;
            serializer.Serialize(writer, sensorId.Id);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var id = serializer.Deserialize<Guid>(reader);
            return new SensorId(id);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SensorId);
        }
    }
}