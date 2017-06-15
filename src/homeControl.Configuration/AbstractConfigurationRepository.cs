using System;

namespace homeControl.Configuration
{
    public abstract class AbstractConfigurationRepository<TConfigurationSource, TConfigurationStore>
    {
        private readonly Lazy<TConfigurationStore> _configuration;
        protected TConfigurationStore Configuration => _configuration.Value;

        protected AbstractConfigurationRepository(
            string configKey,
            IConfigurationLoader<TConfigurationSource> configLoader, 
            Func<TConfigurationSource, TConfigurationStore> configurationPreprocessor)
        {
            Guard.DebugAssertArgumentNotNull(configLoader, nameof(configLoader));
            Guard.DebugAssertArgumentNotNull(configurationPreprocessor, nameof(configurationPreprocessor));
            Guard.DebugAssertArgumentNotNull(configKey, nameof(configKey));

            _configuration = new Lazy<TConfigurationStore>(() => configurationPreprocessor(configLoader.Load(configKey).Result));
        }
    }
}