using System;
using System.Collections.Generic;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Sensors;
using Serilog;

namespace homeControl.SensorEmulator.ViewModels.Sensors
{
    internal sealed class ToggleSensorViewModel : SensorViewModelBaseOfT<bool>
    {
        public ToggleSensorViewModel(IEventSource eventSource,
            IEventSender eventSender, 
            ILogger log) : base(eventSource, eventSender, log)
        {
        }

        protected override bool GetMinimumValue() => false;

        protected override bool GetMaximumValue() => true;

        protected override bool GetMouseWheelUpValue() => true;

        protected override bool GetMouseWheelDownValue() => false;

        protected override bool GetValueFromEvent(AbstractSensorEvent e)
        {
            if (e.SensorId != Id)
                return Value;

            if (e is SensorDeactivatedEvent)
                return false;

            if (e is SensorActivatedEvent)
                return true;

            throw new ArgumentOutOfRangeException(nameof(e));
        }

        protected override IEnumerable<AbstractSensorEvent> GetEventsFromValue(bool value)
        {
            if (value)
            {
                yield return new SensorActivatedEvent(Id);
            }
            else
            {
                yield return new SensorDeactivatedEvent(Id);
            }
        }
    }
}