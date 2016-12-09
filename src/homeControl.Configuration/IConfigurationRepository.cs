using System;

namespace homeControl.Configuration
{
    public interface IConfigurationRepository
    {
        TConfig GetSwicthConfig<TConfig>(Guid switchId);
    }
}