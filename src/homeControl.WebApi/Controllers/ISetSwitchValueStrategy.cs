using System;
using homeControl.Events.Switches;
using homeControl.WebApi.Dto;

namespace homeControl.WebApi.Controllers
{
    public interface ISetSwitchValueStrategy
    {
        bool CanHandle(SwitchKind switchKind, object value);
        SetPowerEvent CreateSetPowerEvent(Guid id, object value);
        AbstractSwitchEvent CreateControlEvent(Guid id, object value);
    }
}