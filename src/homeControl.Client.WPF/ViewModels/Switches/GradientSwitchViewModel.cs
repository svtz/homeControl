using System;
using System.Collections.Generic;
using homeControl.Domain;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Switches;
using Serilog;

namespace homeControl.Client.WPF.ViewModels.Switches
{
    internal class GradientSwitchViewModel : SwitchViewModelBaseOfT<double>
    {
        private const double WheelStep = 0.1d;


        public GradientSwitchViewModel(IEventSource eventSource, IEventSender eventSender, SensorId[] sensors, ILogger log) 
            : base(eventSource, eventSender, sensors, log)
        {
        }

        protected override double GetMinimumValue() => SetSwitchPowerEvent.MinPower;

        protected override double GetMaximumValue() => SetSwitchPowerEvent.MaxPower;

        protected override double GetMouseWheelUpValue() 
            => Math.Min(SetSwitchPowerEvent.MaxPower, Value + WheelStep);

        protected override double GetMouseWheelDownValue()
            => Math.Max(SetSwitchPowerEvent.MinPower, Value - WheelStep);

        protected override double GetValueFromEvent(AbstractSwitchEvent e)
        {
            if (e.SwitchId != Id)
                return Value;

            if (e is TurnSwitchOffEvent)
                return SetSwitchPowerEvent.MinPower;

            if (e is TurnSwitchOnEvent)
                return SetSwitchPowerEvent.MaxPower;

            if (e is SetSwitchPowerEvent powerEvent)
                return powerEvent.Power;

            throw new ArgumentOutOfRangeException(nameof(e));
        }

        protected override IEnumerable<AbstractSwitchEvent> GetEventsFromValue(double value)
        {
            if (value >= SetSwitchPowerEvent.MaxPower)
                yield return new TurnSwitchOnEvent(Id);
            
            yield return new SetSwitchPowerEvent(Id, value);
            
            if (value <= SetSwitchPowerEvent.MinPower)
                yield return new TurnSwitchOffEvent(Id);
        }
    }
}
