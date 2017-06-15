using homeControl.Configuration.Sensors;
using homeControl.Configuration.Switches;

namespace homeControl.Events.Bindings
{
    public class EnableBindingEvent : AbstractBindingEvent
    {
        public EnableBindingEvent(SwitchId switchId, SensorId sensorId) : base(switchId, sensorId)
        {
        }
    }
}