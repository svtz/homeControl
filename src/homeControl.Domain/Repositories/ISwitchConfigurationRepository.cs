using System.Collections.Generic;

namespace homeControl.Domain.Repositories
{
    public interface ISwitchConfigurationRepository
    {
        bool ContainsConfig<TConfig>(SwitchId switchId) where TConfig: ISwitchConfiguration;
        TConfig GetConfig<TConfig>(SwitchId switchId) where TConfig: ISwitchConfiguration;
        IReadOnlyCollection<ISwitchConfiguration> GetAll();
    }
}