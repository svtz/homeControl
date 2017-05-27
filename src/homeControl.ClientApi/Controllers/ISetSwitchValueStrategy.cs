using homeControl.ClientServerShared.Switches;
using homeControl.Configuration.Switches;
using homeControl.Events.Switches;

namespace homeControl.ClientApi.Controllers
{
    public interface ISetSwitchValueStrategy
    {
        bool CanHandle(SwitchKind switchKind, object value);
        SetPowerEvent CreateSetPowerEvent(SwitchId id, object value);
        AbstractSwitchEvent CreateControlEvent(SwitchId id, object value);
    }
}