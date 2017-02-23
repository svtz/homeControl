using System;
using homeControl.Configuration.Switches;
using homeControl.Events.Switches;
using homeControl.WebApi.Dto;

namespace homeControl.WebApi.Controllers
{
    public interface ISetSwitchValueStrategy
    {
        bool CanHandle(SwitchKind switchKind, object value);
        SetPowerEvent CreateSetPowerEvent(SwitchId id, object value);
        AbstractSwitchEvent CreateControlEvent(SwitchId id, object value);
    }
}