using System;
using homeControl.Peripherals;

namespace homeControl.Application
{
    internal sealed class SwitchControllerConsoleEmulator : ISwitchController
    {
        public bool CanHandleSwitch(Guid switchId)
        {
            return true;
        }

        public void TurnOn(Guid switchId)
        {
            Guard.DebugAssertArgumentNotDefault(switchId, nameof(switchId));
            Console.WriteLine($"{switchId}: TurnOn");
        }

        public void TurnOff(Guid switchId)
        {
            Guard.DebugAssertArgumentNotDefault(switchId, nameof(switchId));
            Console.WriteLine($"{switchId}: TurnOff");
        }
    }
}