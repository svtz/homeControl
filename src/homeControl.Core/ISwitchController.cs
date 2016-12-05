using System;

namespace homeControl.Core
{
    public interface ISwitchController
    {
        void TurnOn(Guid switchId);
        void TurnOff(Guid switchId);
    }
}