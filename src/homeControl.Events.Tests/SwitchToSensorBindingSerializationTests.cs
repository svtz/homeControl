using System;
using homeControl.Configuration.Bindings;
using homeControl.Configuration.Sensors;
using homeControl.Configuration.Switches;
using homeControl.Events.Bindings.Configuration;
using Newtonsoft.Json;
using Xunit;

namespace homeControl.Events.Tests
{
    public class SwitchToSensorBindingSerializationTests
    {
        [Fact]
        public void TestJsonSerialization()
        {
            var sourceObj = new SwitchToSensorBinding(SwitchId.NewId(), SensorId.NewId());
            var serializerSettings = new JsonSerializerSettings()
            {
                Converters = new JsonConverter[]
                {
                    new SwitchIdSerializer(),
                    new SensorIdSerializer(),
                    new SwitchToSensorBindingSerializer()
                },
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
            var serialized = JsonConvert.SerializeObject(sourceObj, serializerSettings);

            var resultObj = JsonConvert.DeserializeObject<ISwitchToSensorBinding>(serialized, serializerSettings);

            Assert.NotNull(resultObj);
            Assert.Equal(sourceObj.SwitchId, resultObj.SwitchId);
            Assert.Equal(sourceObj.SensorId, resultObj.SensorId);
        }
    }
}
