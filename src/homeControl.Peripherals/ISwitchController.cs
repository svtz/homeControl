using System;

namespace homeControl.Peripherals
{
    public interface ISwitchController
    {
        bool CanHandleSwitch(Guid switchId);
        void TurnOn(Guid switchId);
        void TurnOff(Guid switchId);
    }
}