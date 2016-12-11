using System;
using homeControl.Configuration.Switches;

namespace homeControl.Peripherals
{
    public interface ISwitchController
    {
        bool CanHandleSwitch(SwitchId switchId);
        void TurnOn(SwitchId switchId);
        void TurnOff(SwitchId switchId);
    }
}