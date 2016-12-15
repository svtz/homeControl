using homeControl.Configuration.Sensors;

namespace homeControl.Noolite.Configuration
{
    internal sealed class NooliteSensorConfig : ISensorConfiguration
    {
        public byte Channel { get; set; }
        public SensorId SensorId { get; set; }
    }
}