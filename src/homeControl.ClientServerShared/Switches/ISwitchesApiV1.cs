using System;

namespace homeControl.ClientServerShared.Switches
{
    public interface ISwitchesApiV1
    {
        SwitchDto[] GetDescriptions();
        bool SetValue(Guid id, object value);
        bool TurnOn(Guid id);
        bool TurnOff(Guid id);
        bool EnableAutomation(Guid id);
        bool DisableAutomation(Guid id);
    }
}