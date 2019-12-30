using System;
using System.Collections.Generic;
using homeControl.Domain;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Switches;
using Serilog;

namespace homeControl.Client.WPF.ViewModels.Switches
{
    internal sealed class ToggleSwitchViewModel : SwitchViewModelBaseOfT<bool>
    {
        public ToggleSwitchViewModel(IEventSource eventSource,
            IEventSender eventSender,
            SensorId[] sensors,
            ILogger log) : base(eventSource, eventSender, sensors, log)
        {
        }

        protected override bool GetMinimumValue() => false;

        protected override bool GetMaximumValue() => true;

        protected override bool GetMouseWheelUpValue() => true;

        protected override bool GetMouseWheelDownValue() => false;

        protected override bool GetValueFromEvent(AbstractSwitchEvent e)
        {
            if (e.SwitchId != Id)
                return Value;

            if (e is TurnSwitchOffEvent)
                return false;

            if (e is TurnSwitchOnEvent)
                return true;

            if (e is SetSwitchPowerEvent powerEvent)
                return powerEvent.Power >= 0.5;

            throw new ArgumentOutOfRangeException(nameof(e));
        }

        protected override IEnumerable<AbstractSwitchEvent> GetEventsFromValue(bool value)
        {
            if (value)
            {
                yield return new TurnSwitchOnEvent(Id);
            }
            else
            {
                yield return new TurnSwitchOffEvent(Id);
            }
        }
    }
}