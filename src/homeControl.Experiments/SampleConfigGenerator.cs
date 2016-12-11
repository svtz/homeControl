using System.IO;
using homeControl.Configuration;
using homeControl.Configuration.Sensors;
using homeControl.Configuration.Switches;
using homeControl.Noolite.Configuration;
using Newtonsoft.Json;

namespace homeControl.Experiments
{
    class SampleConfigGenerator
    {
        public void Run()
        {
            var config = new JsonConfigurationStore();

            config.SwitchConfigurations = new ISwitchConfiguration[]
            {
                new NooliteSwitchConfig() { SwitchId = SwitchId.NewId(), Channel = 1 } 
            };

            config.SensorConfigurations = new ISensorConfiguration[]
            {
                new NooliteSensorConfig { Channel = 0, SensorId = SensorId.NewId() }, 
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