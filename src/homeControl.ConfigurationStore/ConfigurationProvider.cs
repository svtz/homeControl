using System;
using System.IO;
using JetBrains.Annotations;
using Serilog;

namespace homeControl.ConfigurationStore
{
    [UsedImplicitly]
    internal sealed class ConfigurationProvider
    {
        private readonly string _configurationsDirectory;
        private readonly ILogger _logger;

        public ConfigurationProvider(string configurationsDirectory, ILogger logger)
        {
            Guard.DebugAssertArgumentNotNull(configurationsDirectory, nameof(configurationsDirectory));
            _configurationsDirectory = configurationsDirectory;
            _logger = logger;
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
            catch (IOException ex)
            {
                _logger.Error(ex, "Error reading configuration {key}", key);
                return null;
            }
        }
    }
}
