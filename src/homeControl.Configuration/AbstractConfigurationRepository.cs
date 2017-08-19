using System;
using System.Threading.Tasks;

namespace homeControl.Configuration
{
    public abstract class AbstractConfigurationRepository<TConfigurationSource, TConfigurationStore>
    {
        private readonly string _configKey;
        private readonly IConfigurationLoader<TConfigurationSource> _configLoader;
        private readonly Func<TConfigurationSource, TConfigurationStore> _configurationPreprocessor;
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

                _configurationLoader = Task.Run(async () =>
                {
                    var config = await _configLoader.Load(_configKey);
                    return _configurationPreprocessor(config);
                });
            }

            return _configurationLoader;
        }

        protected AbstractConfigurationRepository(
            string configKey,
            IConfigurationLoader<TConfigurationSource> configLoader, 
            Func<TConfigurationSource, TConfigurationStore> configurationPreprocessor)
        {
            Guard.DebugAssertArgumentNotNull(configLoader, nameof(configLoader));
            Guard.DebugAssertArgumentNotNull(configurationPreprocessor, nameof(configurationPreprocessor));
            Guard.DebugAssertArgumentNotNull(configKey, nameof(configKey));

            _configKey = configKey;
            _configLoader = configLoader;
            _configurationPreprocessor = configurationPreprocessor;
        }
    }
}