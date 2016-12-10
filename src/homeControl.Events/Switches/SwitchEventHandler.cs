using System;
using homeControl.Core;
using homeControl.Peripherals;

namespace homeControl.Events.Switches
{
    internal sealed class SwitchEventHandler : IHandler
    {
        private readonly ISwitchController _switchController;
        private Guid _switchId;

        public Guid SwitchId
        {
            get { return _switchId; }
            set
            {
                Guard.DebugAssertArgumentNotDefault(value, nameof(value));
                _switchId = value;
            }
        }

        public SwitchEventHandler(ISwitchController switchController)
        {
            Guard.DebugAssertArgumentNotNull(switchController, nameof(switchController));

            _switchController = switchController;
        }

        public bool CanHandle(IEvent @event)
        {
            Guard.DebugAssertArgumentNotNull(@event, nameof(@event));

            var switchEvent = @event as AbstractSwitchEvent;
            return switchEvent != null && switchEvent.SwitchId == SwitchId && _switchController.CanHandleSwitch(switchEvent.SwitchId);
        }

        public void Handle(IEvent @event)
        {
            Guard.DebugAssertArgumentNotNull(@event, nameof(@event));
            Guard.DebugAssertArgument(CanHandle(@event), nameof(@event));

            if (@event is TurnOnEvent)
            {
                _switchController.TurnOn(SwitchId);
            }
            else if (@event is TurnOffEvent)
            {
                _switchController.TurnOff(SwitchId);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
