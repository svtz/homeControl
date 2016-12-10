using System;
using System.Collections.Generic;
using System.IO;
using homeControl.Configuration;
using homeControl.Noolite.Configuration;
using Newtonsoft.Json;

namespace homeControl.Experiments
{
    class SampleConfigGenerator
    {
        public void Run()
        {
            var config = new JsonConfigurationStore();

            config.SwitchConfigurations = new Dictionary<Guid, ISwitchConfiguration>
            {
                { Guid.NewGuid(), new NooliteSwitchConfig() { Channel = 1 } }
            };

            config.SensorConfigurations = new ISensorConfiguration[]
            {
                new NooliteSensorConfig { Channel = 0, SensorId = Guid.NewGuid() }, 
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