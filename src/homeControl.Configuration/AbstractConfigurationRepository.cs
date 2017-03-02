using System;

namespace homeControl.Configuration
{
    public abstract class AbstractConfigurationRepository<TConfigurationSource, TConfigurationStore>
    {
        private readonly Lazy<TConfigurationStore> _configuration;
        protected TConfigurationStore Configuration => _configuration.Value;

        protected AbstractConfigurationRepository(
            IConfigurationLoader<TConfigurationSource> configLoader, 
            Func<TConfigurationSource, TConfigurationStore> configurationPreprocessor,
            string configFileName)
        {
            Guard.DebugAssertArgumentNotNull(configLoader, nameof(configLoader));

            _configuration = new Lazy<TConfigurationStore>(() => configurationPreprocessor(configLoader.Load(configFileName)));
        }
    }
}