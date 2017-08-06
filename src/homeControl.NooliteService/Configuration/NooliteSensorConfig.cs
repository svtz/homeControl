using homeControl.Configuration.Sensors;

namespace homeControl.NooliteService.Configuration
{
    internal sealed class NooliteSensorConfig : ISensorConfiguration
    {
        public byte Channel { get; set; }
        public SensorId SensorId { get; set; }
    }
}