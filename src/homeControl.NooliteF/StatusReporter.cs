using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using homeControl.Domain.Configuration;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Switches;
using homeControl.Domain.Repositories;
using homeControl.NooliteF.Configuration;
using homeControl.NooliteF.SwitchController;
using Serilog;

namespace homeControl.NooliteF
{
    internal sealed class StatusReporter
    {
        private readonly NooliteFSwitchesStatusHolder _statusHolder;
        private readonly IEventSource _source;
        private readonly IEventSender _sender;
        private readonly ILogger _log;
        private readonly ISwitchConfigurationRepository _switches;
        private readonly INooliteFSwitchInfoRepository _nooliteSwitches;

        public StatusReporter(NooliteFSwitchesStatusHolder statusHolder,
            IEventSource source,
            IEventSender sender,
            ILogger log,
            ISwitchConfigurationRepository switches,
            INooliteFSwitchInfoRepository nooliteSwitches)
        {
            Guard.DebugAssertArgumentNotNull(statusHolder, nameof(statusHolder));
            Guard.DebugAssertArgumentNotNull(source, nameof(source));
            Guard.DebugAssertArgumentNotNull(log, nameof(log));

            _statusHolder = statusHolder;
            _source = source;
            _sender = sender;
            _log = log;
            _switches = switches;
            _nooliteSwitches = nooliteSwitches;
        }

        
        public Task Run(CancellationToken ct)
        {
            return _source
                .ReceiveEvents<NeedStatusEvent>()
                .ForEachAsync(ProcessRequest, ct);
        }

        private readonly object _lock = new object();
        private void ProcessRequest(NeedStatusEvent obj)
        {
            _log.Debug("Processing NeedStatusEvent");
            lock (_lock)
            {
                foreach (var @switch in _nooliteSwitches.GetAll().Result)
                {
                    _log.Debug("Sending {id}", @switch.SwitchId);
                    SendSwitchStatus(@switch);
                }
            }
        }

        private void SendSwitchStatus(NooliteFSwitchInfo nooliteSwitch)
        {
            var status = _statusHolder.GetStatus(nooliteSwitch.Channel);
            if (status == null)
                return;

            var @switch = _switches.GetConfig(nooliteSwitch.SwitchId).Result;
            
            switch (@switch.SwitchKind)
            {
                case SwitchKind.Toggle:
                    _sender.SendEvent(status.Value.isOn ? (IEvent)new TurnSwitchOnEvent(@switch.SwitchId) : new TurnSwitchOffEvent(@switch.SwitchId));
                    break;
                case SwitchKind.Gradient:
                    _sender.SendEvent(new SetSwitchPowerEvent(@switch.SwitchId, 
                        status.Value.power == 0 ? 0 : (nooliteSwitch.FullPowerLevel - nooliteSwitch.ZeroPowerLevel) + nooliteSwitch.ZeroPowerLevel / status.Value.power));
                    _sender.SendEvent(status.Value.isOn ? (IEvent)new TurnSwitchOnEvent(@switch.SwitchId) : new TurnSwitchOffEvent(@switch.SwitchId));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}