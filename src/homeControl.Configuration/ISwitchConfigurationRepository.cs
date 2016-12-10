using System;

namespace homeControl.Configuration
{
    public interface ISwitchConfigurationRepository
    {
        bool ContainsConfig<TConfig>(Guid switchId) where TConfig: ISwitchConfiguration;
        TConfig GetConfig<TConfig>(Guid switchId) where TConfig: ISwitchConfiguration;
        Guid[] GetAllIds();
    }
}