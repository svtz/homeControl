using System;

namespace homeControl.Peripherals
{
    public sealed class SensorEventArgs : EventArgs
    {
        public Guid SensorId { get; }

        public SensorEventArgs(Guid sensorId)
        {
            Guard.DebugAssertArgumentNotDefault(sensorId, nameof(sensorId));
            SensorId = sensorId;
        }
    }
}