using System.Collections.Generic;

namespace homeControl.Domain.Repositories
{
    public interface ISensorConfigurationRepository
    {
        IReadOnlyCollection<TConfig> GetAll<TConfig>() where TConfig : ISensorConfiguration;
    }
}