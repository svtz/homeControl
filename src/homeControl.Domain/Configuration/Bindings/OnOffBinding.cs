using System;
using System.Diagnostics;

namespace homeControl.Domain.Configuration.Bindings
{
    [DebuggerDisplay("SwitchId::SensorId::Mode")]
    public sealed class OnOffBinding : SwitchToSensorBinding, IEquatable<OnOffBinding>
    {
        public OnOffBindingMode Mode { get; set; }

        public bool Equals(OnOffBinding other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Mode == other.Mode;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is OnOffBinding other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (int) Mode;
            }
        }

        public static bool operator ==(OnOffBinding left, OnOffBinding right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(OnOffBinding left, OnOffBinding right)
        {
            return !Equals(left, right);
        }
    }
}