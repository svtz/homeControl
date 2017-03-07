using System;
using homeControl.WebApi.Dto;

namespace homeControl.ClientServerShared
{
    public interface ISwitchesApi
    {
        SwitchDto[] GetDescriptions();
        bool SetValue(Guid id, object value);
        bool TurnOn(Guid id);
        bool TurnOff(Guid id);
        bool EnableAutomation(Guid id);
        bool DisableAutomation(Guid id);
    }
}