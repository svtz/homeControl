using System.Diagnostics;

namespace homeControl.Domain.Configuration
{
    [DebuggerDisplay("SensorId")]
    public sealed class SensorConfiguration
    {
        public SensorId SensorId { get; set; }
    }
}