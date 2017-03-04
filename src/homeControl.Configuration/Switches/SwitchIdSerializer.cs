using System;
using Newtonsoft.Json;

namespace homeControl.Configuration.Switches
{
    internal sealed class SwitchIdSerializer : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var switchId = (SwitchId)value;
            serializer.Serialize(writer, switchId.Id);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var id = serializer.Deserialize<Guid>(reader);
            return new SwitchId(id);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SwitchId);
        }
    }
}