using System.Collections.Generic;

namespace homeControl.Configuration.Sensors
{
    public interface ISensorConfigurationRepository
    {
        IReadOnlyCollection<TConfig> GetAll<TConfig>() where TConfig : ISensorConfiguration;
    }
}