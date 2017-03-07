using System;
using System.Collections.Generic;

namespace homeControl.WebApi.Configuration
{
    internal interface IClientApiConfigurationRepository
    {
        IReadOnlyCollection<SwitchApiConfig> GetAll();
        SwitchApiConfig TryGetById(Guid id);
    }
}