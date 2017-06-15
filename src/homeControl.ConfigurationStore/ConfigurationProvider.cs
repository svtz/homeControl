using System;
using System.IO;
using JetBrains.Annotations;

namespace homeControl.ConfigurationStore
{
    [UsedImplicitly]
    internal sealed class ConfigurationProvider
    {
        private readonly string _configurationsDirectory;

        public ConfigurationProvider(string configurationsDirectory)
        {
            Guard.DebugAssertArgumentNotNull(configurationsDirectory, nameof(configurationsDirectory));
            _configurationsDirectory = configurationsDirectory;
        }

        public string GetConfiguration(string key)
        {
            Guard.DebugAssertArgumentNotNull(key, nameof(key));

            var configFileName = $"{key}.json";
            var fullPath = Path.Combine(_configurationsDirectory, configFileName);

            try
            {
                return File.ReadAllText(fullPath);
            }
            catch (IOException)
            {
                return null;
            }
        }
    }
}
