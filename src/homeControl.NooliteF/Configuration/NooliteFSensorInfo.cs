using homeControl.Domain;

namespace homeControl.NooliteF.Configuration
{
    internal sealed class NooliteFSensorInfo
    {
        public byte Channel { get; set; }
        public SensorId SensorId { get; set; }
    }
}