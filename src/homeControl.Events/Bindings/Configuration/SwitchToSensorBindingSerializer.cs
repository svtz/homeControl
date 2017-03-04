using System;
using System.Reflection;
using homeControl.Configuration.Sensors;
using homeControl.Configuration.Switches;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace homeControl.Events.Bindings.Configuration
{
    internal sealed class SwitchToSensorBindingSerializer : JsonConverter
    {
        private const string TypeName = "$type";

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var binding = (SwitchToSensorBinding)value;

            writer.WriteStartObject();
            writer.WritePropertyName(TypeName);
            serializer.Serialize(writer, GetTypeName(binding.GetType()));
            writer.WritePropertyName(nameof(SwitchToSensorBinding.SwitchId));
            serializer.Serialize(writer, binding.SwitchId);
            writer.WritePropertyName(nameof(SwitchToSensorBinding.SensorId));
            serializer.Serialize(writer, binding.SensorId);
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.Null)
            {
                var token = JToken.Load(reader);
                
                var typeName = token.Value<string>(TypeName);
                if (typeName != GetTypeName(objectType))
                {
                    return CreateEmptyBinding();
                }
                var switchId = token.Value<SwitchId>(nameof(SwitchToSensorBinding.SwitchId));
                var sensorId = token.Value<SensorId>(nameof(SwitchToSensorBinding.SensorId));

                return new SwitchToSensorBinding(switchId, sensorId);
            }

            return CreateEmptyBinding();
        }

        private static string GetTypeName(Type type)
        {
            return $"{type.FullName}, {type.GetTypeInfo().Assembly.GetName().Name}";
        }
            

        private static SwitchToSensorBinding CreateEmptyBinding()
        {
            return new SwitchToSensorBinding(new SwitchId(Guid.Empty), new SensorId(Guid.Empty));
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SwitchToSensorBinding);
        }
    }
}