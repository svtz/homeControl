using System;
using homeControl.ClientServerShared.Dto;

namespace homeControl.ClientServerShared
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