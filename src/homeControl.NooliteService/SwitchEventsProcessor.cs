using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using homeControl.Domain.Events;
using homeControl.Events.Switches;
using homeControl.Peripherals;
using JetBrains.Annotations;

namespace homeControl.NooliteService
{
    [UsedImplicitly]
    internal sealed class SwitchEventsProcessor
    {
        private readonly ISwitchController _switchController;
        private readonly IEventSource _source;

        public SwitchEventsProcessor(ISwitchController switchController, IEventSource source)
        {
            Guard.DebugAssertArgumentNotNull(switchController, nameof(switchController));
            Guard.DebugAssertArgumentNotNull(source, nameof(source));

            _switchController = switchController;
            _source = source;
        }
        
        public Task Run(CancellationToken ct)
        {
            return _source.GetMessages<AbstractSwitchEvent>().ForEachAsync(HandleEvent, ct);
        }

        private void HandleEvent(AbstractSwitchEvent switchEvent)
        {
            Guard.DebugAssertArgumentNotNull(switchEvent, nameof(switchEvent));

            if (!_switchController.CanHandleSwitch(switchEvent.SwitchId))
            {
                return;
            }

            if (switchEvent is TurnOnEvent)
            {
                _switchController.TurnOn(switchEvent.SwitchId);
            }
            else if (switchEvent is TurnOffEvent)
            {
                _switchController.TurnOff(switchEvent.SwitchId);
            }
            else if (switchEvent is SetPowerEvent)
            {
                var setPower = (SetPowerEvent)switchEvent;
                _switchController.SetPower(switchEvent.SwitchId, setPower.Power);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
