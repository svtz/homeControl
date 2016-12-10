using System;

namespace homeControl.Configuration
{
    public interface ISwitchConfigurationRepository
    {
        TConfig GetSwicthConfig<TConfig>(Guid switchId);
    }
}