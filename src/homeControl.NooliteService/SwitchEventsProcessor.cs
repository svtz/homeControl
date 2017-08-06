using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Switches;
using homeControl.NooliteService.SwitchController;
using JetBrains.Annotations;
using Serilog;

namespace homeControl.NooliteService
{
    [UsedImplicitly]
    internal sealed class SwitchEventsProcessor
    {
        private readonly ISwitchController _switchController;
        private readonly IEventSource _source;
        private readonly ILogger _log;

        public SwitchEventsProcessor(ISwitchController switchController, IEventSource source, ILogger log)
        {
            Guard.DebugAssertArgumentNotNull(switchController, nameof(switchController));
            Guard.DebugAssertArgumentNotNull(source, nameof(source));

            _switchController = switchController;
            _source = source;
            _log = log;
        }
        
        public Task Run(CancellationToken ct)
        {
            return _source.ReceiveEvents<AbstractSwitchEvent>().ForEachAsync(HandleEvent, ct);
        }

        private void HandleEvent(AbstractSwitchEvent switchEvent)
        {
            Guard.DebugAssertArgumentNotNull(switchEvent, nameof(switchEvent));

            if (!_switchController.CanHandleSwitch(switchEvent.SwitchId))
            {
                _log.Debug("Switch not supported: {SwitchId}", switchEvent.SwitchId);
                return;
            }

            if (switchEvent is TurnOnEvent)
            {
                _switchController.TurnOn(switchEvent.SwitchId);
                _log.Information("Switch turned on: {SwitchId}", switchEvent.SwitchId);
            }
            else if (switchEvent is TurnOffEvent)
            {
                _switchController.TurnOff(switchEvent.SwitchId);
                _log.Information("Switch turned off: {SwitchId}", switchEvent.SwitchId);
            }
            else if (switchEvent is SetPowerEvent setPower)
            {
                _switchController.SetPower(switchEvent.SwitchId, setPower.Power);
                _log.Information("Adjusted switch power: {SwitchId}, {Power:G}", setPower.SwitchId, setPower.Power);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
