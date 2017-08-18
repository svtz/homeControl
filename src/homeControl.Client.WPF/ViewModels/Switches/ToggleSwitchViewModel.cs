using System;
using System.Collections.Generic;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Switches;
using Serilog;

namespace homeControl.Client.WPF.ViewModels.Switches
{
    internal sealed class ToggleSwitchViewModel : SwitchViewModelBaseOfT<bool>
    {
        public ToggleSwitchViewModel(IEventSource eventSource,
            IEventSender eventSender, 
            ILogger log) : base(eventSource, eventSender, log)
        {
        }

        protected override void OnMouseWheelUp()
        {
            Value = true;
        }

        protected override void OnMouseWheelDown()
        {
            Value = false;
        }

        protected override void SetEvent(AbstractSwitchEvent e)
        {
            if (e.SwitchId != Id)
                return;

            if (e is TurnOffEvent)
                Value = false;
            else if (e is TurnOnEvent)
                Value = true;
            else if (e is SetPowerEvent powerEvent)
                Value = powerEvent.Power >= 0.5;
            else
            {
                throw new ArgumentOutOfRangeException(nameof(e));
            }
        }

        protected override IEnumerable<AbstractSwitchEvent> GetEvent(bool value)
        {
            if (value)
            {
                yield return new TurnOnEvent(Id);
            }
            else
            {
                yield return new TurnOffEvent(Id);
            }
        }

        protected override void OnSetMaximum()
        {
            Value = true;
        }

        protected override void OnSetMinimum()
        {
            Value = false;
        }
    }
}