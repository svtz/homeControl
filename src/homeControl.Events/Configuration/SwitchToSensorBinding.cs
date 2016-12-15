using homeControl.Configuration.Bindings;
using homeControl.Configuration.Sensors;
using homeControl.Configuration.Switches;

namespace homeControl.Events.Configuration
{
    public sealed class SwitchToSensorBinding : ISwitchToSensorBinding
    {
        public SwitchId SwitchId { get; set; }
        public SensorId SensorId { get; set; }
    }
}
