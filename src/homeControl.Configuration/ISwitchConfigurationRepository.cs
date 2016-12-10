using System;

namespace homeControl.Configuration
{
    public interface ISwitchConfigurationRepository
    {
        bool ContainsConfig<TConfig>(Guid switchId);
        TConfig GetConfig<TConfig>(Guid switchId);
    }
}