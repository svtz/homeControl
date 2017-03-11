using System;
using System.Collections.Generic;

namespace homeControl.ClientApi.Configuration
{
    internal interface IClientApiConfigurationRepository
    {
        IReadOnlyCollection<SwitchApiConfig> GetAll();
        SwitchApiConfig TryGetById(Guid id);
    }
}