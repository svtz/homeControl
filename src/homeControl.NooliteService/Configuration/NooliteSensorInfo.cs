using homeControl.Domain;

namespace homeControl.NooliteService.Configuration
{
    internal sealed class NooliteSensorInfo
    {
        public byte Channel { get; set; }
        public SensorId SensorId { get; set; }
    }
}