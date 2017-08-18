using System;
using System.Collections.Generic;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Switches;
using Serilog;

namespace homeControl.Client.WPF.ViewModels.Switches
{
    internal class GradientSwitchViewModel : SwitchViewModelBaseOfT<double>
    {
        private const double WheelStep = 0.1d;


        public GradientSwitchViewModel(IEventSource eventSource, IEventSender eventSender, ILogger log) : base(eventSource, eventSender, log)
        {
        }

        protected override void OnMouseWheelUp()
        {
            Value = Math.Min(SetPowerEvent.MaxPower, Value + WheelStep);
        }

        protected override void OnMouseWheelDown()
        {
            Value = Math.Max(SetPowerEvent.MinPower, Value - WheelStep);
        }

        protected override void OnSetMaximum()
        {
            Value = SetPowerEvent.MaxPower;
        }

        protected override void OnSetMinimum()
        {
            Value = SetPowerEvent.MinPower;
        }

        protected override void SetEvent(AbstractSwitchEvent e)
        {
            if (e.SwitchId != Id)
                return;

            if (e is TurnOffEvent)
                Value = SetPowerEvent.MinPower;
            else if (e is TurnOnEvent)
                Value = SetPowerEvent.MaxPower;
            else if (e is SetPowerEvent powerEvent)
                Value = powerEvent.Power;
            else
            {
                throw new ArgumentOutOfRangeException(nameof(e));
            }
        }

        protected override IEnumerable<AbstractSwitchEvent> GetEvent(double value)
        {
            yield return new SetPowerEvent(Id, value);

            if (value <= SetPowerEvent.MinPower)
                yield return new TurnOffEvent(Id);

            if (value >= SetPowerEvent.MaxPower)
                yield return new TurnOnEvent(Id);
        }
    }
}
