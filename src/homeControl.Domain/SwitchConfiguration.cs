using System;
using System.Diagnostics;

namespace homeControl.Domain
{
    [DebuggerDisplay("SwitchId")]
    public sealed class SwitchConfiguration : IEquatable<SwitchConfiguration>
    {
        public SwitchId SwitchId { get; set; }
        public SwitchKind SwitchKind { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }


        public bool Equals(SwitchConfiguration other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(SwitchId, other.SwitchId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is SwitchConfiguration && Equals((SwitchConfiguration)obj);
        }

        public override int GetHashCode()
        {
            return (SwitchId != null ? SwitchId.GetHashCode() : 0);
        }

        public static bool operator ==(SwitchConfiguration left, SwitchConfiguration right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SwitchConfiguration left, SwitchConfiguration right)
        {
            return !Equals(left, right);
        }
    }
}