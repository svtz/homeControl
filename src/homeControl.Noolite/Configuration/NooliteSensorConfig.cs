using System;
using homeControl.Configuration;

namespace homeControl.Noolite.Configuration
{
    internal sealed class NooliteSensorConfig : ISensorConfiguration
    {
        public byte Channel { get; set; }
        public Guid SensorId { get; set; }
    }
}