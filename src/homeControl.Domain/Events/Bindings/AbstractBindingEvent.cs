using homeControl.Configuration.Sensors;
using homeControl.Configuration.Switches;
using homeControl.Domain.Events;

namespace homeControl.Events.Bindings
{
    public abstract class AbstractBindingEvent : IEvent
    {
        public SwitchId SwitchId { get; }
        public SensorId SensorId { get; }

        protected AbstractBindingEvent(SwitchId switchId, SensorId sensorId)
        {
            SwitchId = switchId;
            SensorId = sensorId;
        }
    }
}