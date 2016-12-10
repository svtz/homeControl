using System;
using System.Linq;

namespace homeControl.Peripherals
{
    internal sealed class SwitchControllerSelector : ISwitchControllerSelector
    {
        private readonly ISwitchController[] _controllerImplementations;

        public SwitchControllerSelector(ISwitchController[] controllerImplementations)
        {
            _controllerImplementations = controllerImplementations;
        }

        public bool CanHandleSwitch(Guid switchId)
        {
            Guard.DebugAssertArgumentNotDefault(switchId, nameof(switchId));

            return _controllerImplementations.Any(cntr => cntr.CanHandleSwitch(switchId));

        }

        public void TurnOn(Guid switchId)
        {
            Guard.DebugAssertArgumentNotDefault(switchId, nameof(switchId));
            Guard.DebugAssertArgument(CanHandleSwitch(switchId), nameof(switchId));

            foreach (var controller in _controllerImplementations.Where(cntr => cntr.CanHandleSwitch(switchId)))
            {
                controller.TurnOn(switchId);
            }
        }

        public void TurnOff(Guid switchId)
        {
            Guard.DebugAssertArgumentNotDefault(switchId, nameof(switchId));
            Guard.DebugAssertArgument(CanHandleSwitch(switchId), nameof(switchId));

            foreach (var controller in _controllerImplementations.Where(cntr => cntr.CanHandleSwitch(switchId)))
            {
                controller.TurnOff(switchId);
            }
        }
    }
}
