using System.IO;
using homeControl.Configuration;
using homeControl.Configuration.Bindings;
using homeControl.Configuration.Sensors;
using homeControl.Configuration.Switches;
using homeControl.Events.Configuration;
using homeControl.Noolite.Configuration;
using Newtonsoft.Json;

namespace homeControl.Experiments
{
    class SampleConfigGenerator
    {
        public void Run()
        {
            var config = new JsonConfigurationStore();

            var switchId = SwitchId.NewId();
            config.SwitchConfigurations = new ISwitchConfiguration[]
            {
                new NooliteSwitchConfig() { SwitchId = switchId, Channel = 1 } 
            };

            var sensorId = SensorId.NewId();
            config.SensorConfigurations = new ISensorConfiguration[]
            {
                new NooliteSensorConfig { Channel = 0, SensorId = sensorId }, 
            };

            config.Bindings = new ISwitchToSensorBinding[]
            {
                new SwitchToSensorBinding
                {
                    SwitchId = switchId,
                    SensorId = sensorId
                }
            };

            var configString = JsonConvert.SerializeObject(config, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto
            });

            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "config.json"), configString);
        }
    }
}