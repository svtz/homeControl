using System;

namespace homeControl.WebApi.Configuration
{
    public interface IClientApiConfigurationRepository
    {
        SwitchApiConfig[] GetAll();
        SwitchApiConfig TryGetById(Guid id);
    }
}