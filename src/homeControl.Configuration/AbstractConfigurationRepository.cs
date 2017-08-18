using System;
using System.Threading.Tasks;

namespace homeControl.Configuration
{
    public abstract class AbstractConfigurationRepository<TConfigurationSource, TConfigurationStore>
    {
        private readonly Task<TConfigurationStore> _configurationLoader;
        private readonly object _lock = new object();

        protected async Task<TConfigurationStore> GetConfiguration() => await _configurationLoader;

        protected AbstractConfigurationRepository(
            string configKey,
            IConfigurationLoader<TConfigurationSource> configLoader, 
            Func<TConfigurationSource, TConfigurationStore> configurationPreprocessor)
        {
            Guard.DebugAssertArgumentNotNull(configLoader, nameof(configLoader));
            Guard.DebugAssertArgumentNotNull(configurationPreprocessor, nameof(configurationPreprocessor));
            Guard.DebugAssertArgumentNotNull(configKey, nameof(configKey));

            _configurationLoader = Task.Run(async () =>
            {
                var config = await configLoader.Load(configKey);
                return configurationPreprocessor(config);
            });
        }
    }
}