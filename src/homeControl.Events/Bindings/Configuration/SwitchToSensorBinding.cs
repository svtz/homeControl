using System;
using System.Diagnostics;
using homeControl.Configuration.Bindings;
using homeControl.Configuration.Sensors;
using homeControl.Configuration.Switches;

namespace homeControl.Events.Bindings.Configuration
{
    [DebuggerDisplay("SwitchId::SensorId")]
    internal sealed class SwitchToSensorBinding : ISwitchToSensorBinding, IEquatable<SwitchToSensorBinding>
    {
        public SwitchId SwitchId { get; }
        public SensorId SensorId { get; }

        public SwitchToSensorBinding(SwitchId switchId, SensorId sensorId)
        {
            Guard.DebugAssertArgumentNotNull(switchId, nameof(switchId));
            Guard.DebugAssertArgumentNotNull(sensorId, nameof(sensorId));

            SwitchId = switchId;
            SensorId = sensorId;
        }

        public bool Equals(SwitchToSensorBinding other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return SwitchId.Equals(other.SwitchId) && SensorId.Equals(other.SensorId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is SwitchToSensorBinding && Equals((SwitchToSensorBinding)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (SwitchId.GetHashCode() * 397) ^ SensorId.GetHashCode();
            }
        }

        public static bool operator ==(SwitchToSensorBinding left, SwitchToSensorBinding right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SwitchToSensorBinding left, SwitchToSensorBinding right)
        {
            return !Equals(left, right);
        }
    }
}
