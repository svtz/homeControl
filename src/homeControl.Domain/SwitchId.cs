using System;
using System.Diagnostics;

namespace homeControl.Configuration.Switches
{
    [DebuggerDisplay("Id")]
    public sealed class SwitchId : IEquatable<SwitchId>
    {
        public Guid Id { get; }

        public SwitchId(Guid id)
        {
            Guard.DebugAssertArgumentNotDefault(id, nameof(id));

            Id = id;
        }

        public bool Equals(SwitchId other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as SwitchId;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(SwitchId left, SwitchId right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SwitchId left, SwitchId right)
        {
            return !Equals(left, right);
        }

        public static SwitchId NewId()
        {
            return new SwitchId(Guid.NewGuid());
        }
    }
}