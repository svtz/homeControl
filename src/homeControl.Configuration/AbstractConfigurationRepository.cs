using System;
using System.Threading.Tasks;
using Serilog;

namespace homeControl.Configuration
{
    public abstract class AbstractConfigurationRepository<TConfigurationSource, TConfigurationStore>
    {
        private readonly string _configKey;
        private readonly IConfigurationLoader<TConfigurationSource> _configLoader;
        private readonly Func<TConfigurationSource, TConfigurationStore> _configurationPreprocessor;
        private readonly ILogger _log;
        private volatile Task<TConfigurationStore> _configurationLoader;
        private readonly object _lock = new object();

        protected Task<TConfigurationStore> GetConfiguration()
        {
            if (_configurationLoader != null)
                return _configurationLoader;

            lock (_lock)
            {
                if (_configurationLoader != null)
                    return _configurationLoader;

                _configurationLoader = Task.Run(LoadConfiguration);
            }

            return _configurationLoader;
        }

        private async Task<TConfigurationStore> LoadConfiguration()
        {
            try
            {
                var config = await _configLoader.Load(_configKey);
                return _configurationPreprocessor(config);
            }
            catch (Exception e)
            {
                _log.Error("Error while downloading configuration: {Exception}", e);
                throw;
            }
        }

        protected AbstractConfigurationRepository(
            string configKey,
            IConfigurationLoader<TConfigurationSource> configLoader, 
            Func<TConfigurationSource, TConfigurationStore> configurationPreprocessor,
            ILogger log)
        {
            Guard.DebugAssertArgumentNotNull(configLoader, nameof(configLoader));
            Guard.DebugAssertArgumentNotNull(configurationPreprocessor, nameof(configurationPreprocessor));
            Guard.DebugAssertArgumentNotNull(configKey, nameof(configKey));
            Guard.DebugAssertArgumentNotNull(log, nameof(log));

            _configKey = configKey;
            _configLoader = configLoader;
            _configurationPreprocessor = configurationPreprocessor;
            _log = log;
        }
    }
}