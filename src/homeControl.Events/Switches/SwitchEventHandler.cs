using System;
using homeControl.Core;
using homeControl.Peripherals;

namespace homeControl.Events.Switches
{
    internal sealed class SwitchEventHandler : IHandler
    {
        private readonly ISwitchController _switchController;

        public SwitchEventHandler(ISwitchController switchController)
        {
            Guard.DebugAssertArgumentNotNull(switchController, nameof(switchController));

            _switchController = switchController;
        }

        public bool CanHandle(IEvent @event)
        {
            Guard.DebugAssertArgumentNotNull(@event, nameof(@event));

            var switchEvent = @event as AbstractSwitchEvent;
            return switchEvent != null && _switchController.CanHandleSwitch(switchEvent.SwitchId);
        }

        public void Handle(IEvent @event)
        {
            Guard.DebugAssertArgumentNotNull(@event, nameof(@event));
            Guard.DebugAssertArgument(CanHandle(@event), nameof(@event));

            var switchEvent = @event as AbstractSwitchEvent;
            if (@event is TurnOnEvent)
            {
                _switchController.TurnOn(switchEvent.SwitchId);
            }
            else if (@event is TurnOffEvent)
            {
                _switchController.TurnOff(switchEvent.SwitchId);
            }
            else if (@event is SetPowerEvent)
            {
                var setPower = (SetPowerEvent)@event;
                _switchController.SetPower(switchEvent.SwitchId, setPower.Power);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
