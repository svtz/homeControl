using System;
using homeControl.Configuration.Sensors;

namespace homeControl.Peripherals
{
    public sealed class SensorEventArgs : EventArgs
    {
        public SensorId SensorId { get; }

        public SensorEventArgs(SensorId sensorId)
        {
            Guard.DebugAssertArgumentNotNull(sensorId, nameof(sensorId));
            SensorId = sensorId;
        }
    }
}