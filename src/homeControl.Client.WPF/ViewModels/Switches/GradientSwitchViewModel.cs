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

        protected override double GetMinimumValue() => SetPowerEvent.MinPower;

        protected override double GetMaximumValue() => SetPowerEvent.MaxPower;

        protected override double GetMouseWheelUpValue() 
            => Math.Min(SetPowerEvent.MaxPower, Value + WheelStep);

        protected override double GetMouseWheelDownValue()
            => Math.Max(SetPowerEvent.MinPower, Value - WheelStep);

        protected override double GetValueFromEvent(AbstractSwitchEvent e)
        {
            if (e.SwitchId != Id)
                return Value;

            if (e is TurnOffEvent)
                return SetPowerEvent.MinPower;

            if (e is TurnOnEvent)
                return SetPowerEvent.MaxPower;

            if (e is SetPowerEvent powerEvent)
                return powerEvent.Power;

            throw new ArgumentOutOfRangeException(nameof(e));
        }

        protected override IEnumerable<AbstractSwitchEvent> GetEventsFromValue(double value)
        {
            if (value >= SetPowerEvent.MaxPower)
                yield return new TurnOnEvent(Id);
            
            yield return new SetPowerEvent(Id, value);
            
            if (value <= SetPowerEvent.MinPower)
                yield return new TurnOffEvent(Id);
        }
    }
}
