using homeControl.Configuration.Sensors;
using homeControl.Configuration.Switches;

namespace homeControl.Configuration.Bindings
{
    public interface ISwitchToSensorBinding
    {
        SwitchId SwitchId { get; }
        SensorId SensorId { get; }
    }
}