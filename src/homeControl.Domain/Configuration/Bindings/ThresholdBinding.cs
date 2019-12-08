using System;
using System.Diagnostics;

namespace homeControl.Domain.Configuration.Bindings
{
    [DebuggerDisplay("SwitchId::SensorId::Operator::Threshold")]
    public sealed class ThresholdBinding : SwitchToSensorBinding, IEquatable<ThresholdBinding>
    {
        public decimal Threshold { get; set; }
        public ThresholdOperator Operator { get; set; }

        public bool Equals(ThresholdBinding other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Threshold == other.Threshold && Operator == other.Operator;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is ThresholdBinding other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ Threshold.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) Operator;
                return hashCode;
            }
        }

        public static bool operator ==(ThresholdBinding left, ThresholdBinding right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ThresholdBinding left, ThresholdBinding right)
        {
            return !Equals(left, right);
        }
    }
}