using System;

namespace homeControl.Peripherals
{
    public interface ISwitchController
    {
        void TurnOn(Guid switchId);
        void TurnOff(Guid switchId);
    }
}