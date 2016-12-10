using System;

namespace homeControl.Noolite.Configuration
{
    internal sealed class NooliteSensorConfig
    {
        public byte Channel { get; set; }
        public Guid SensorId { get; set; }
    }
}