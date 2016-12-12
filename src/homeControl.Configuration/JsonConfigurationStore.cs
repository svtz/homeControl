using System.IO;
using homeControl.Configuration.Bindings;
using homeControl.Configuration.Sensors;
using homeControl.Configuration.Switches;
using Newtonsoft.Json;

namespace homeControl.Configuration
{
    internal sealed class JsonConfigurationStore
    {
        public ISwitchConfiguration[] SwitchConfigurations { get; set; }
        public ISensorConfiguration[] SensorConfigurations { get; set; }
        public ISwitchToSensorBinding[] Bindings { get; set; }

        internal static JsonConfigurationStore Load(string configPath)
        {
            if (string.IsNullOrWhiteSpace(configPath))
                throw new InvalidConfigurationException("Path to configuration file should be specified.");

            string content;
            try
            {
                content = File.ReadAllText(configPath);
            }
            catch (IOException ex)
            {
                throw new InvalidConfigurationException(ex, "Unable to read config file.");
            }

            var serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
            };

            try
            {
                return JsonConvert.DeserializeObject<JsonConfigurationStore>(content, serializerSettings);
            }
            catch (JsonException ex)
            {
                throw new InvalidConfigurationException(ex, "Error reading json from the config file.");
            }
        }
    }
}
