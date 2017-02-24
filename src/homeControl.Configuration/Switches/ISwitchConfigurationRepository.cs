using System.Collections.Generic;

namespace homeControl.Configuration.Switches
{
    public interface ISwitchConfigurationRepository
    {
        bool ContainsConfig<TConfig>(SwitchId switchId) where TConfig: ISwitchConfiguration;
        TConfig GetConfig<TConfig>(SwitchId switchId) where TConfig: ISwitchConfiguration;
        IReadOnlyCollection<ISwitchConfiguration> GetAll();
    }
}