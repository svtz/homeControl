using System.IO;
using Newtonsoft.Json;

namespace homeControl.Configuration
{
    internal sealed class JsonConfigurationLoader<TConfiguration> : IConfigurationLoader<TConfiguration>
    {
        private readonly string _configDirectory;

        public JsonConfigurationLoader(string configDirectory)
        {
            Guard.DebugAssertArgumentNotNull(configDirectory, nameof(configDirectory));

            _configDirectory = configDirectory;
        }

        public TConfiguration Load(string fileName)
        {
            Guard.DebugAssertArgumentNotNull(fileName, nameof(fileName));
            var configFile = new FileInfo(Path.Combine(_configDirectory, fileName));

            if (!configFile.Exists)
                throw new InvalidConfigurationException($"Config file \"{configFile.FullName}\" does not exist.");

            string content;
            try
            {
                content = File.ReadAllText(configFile.FullName);
            }
            catch (IOException ex)
            {
                throw new InvalidConfigurationException(ex, "Unable to read config file.");
            }

            var serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
            };

            try
            {
                return JsonConvert.DeserializeObject<TConfiguration>(content, serializerSettings);
            }
            catch (JsonException ex)
            {
                throw new InvalidConfigurationException(ex, $"Error reading json from \"{configFile.FullName}\"");
            }
        }
    }
}
