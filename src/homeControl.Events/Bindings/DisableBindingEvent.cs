using homeControl.Configuration.Sensors;
using homeControl.Configuration.Switches;

namespace homeControl.Events.Bindings
{
    public class DisableBindingEvent : AbstractBindingEvent
    {
        public DisableBindingEvent(SwitchId switchId, SensorId sensorId) : base(switchId, sensorId)
        {
        }
    }
}