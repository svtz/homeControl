using System;
using System.Collections.Generic;

namespace homeControl.WebApi.Configuration
{
    public interface IClientApiConfigurationRepository
    {
        IReadOnlyCollection<SwitchApiConfig> GetAll();
        SwitchApiConfig TryGetById(Guid id);
    }
}