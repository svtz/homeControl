using System.Collections.ObjectModel;
using System.Linq;

namespace ThinkingHome.NooLite.ReceivedData
{
	public abstract class ReceivedCommandData
	{
		public byte[] Buffer { get; }

		protected ReceivedCommandData(byte[] source)
		{
			Buffer = (byte[])source.Clone();
		}

		public bool Binding => (Buffer[0] & 0x40) > 0;

		public byte Cmd => Buffer[2];

		public byte Channel => Buffer[1];

		internal CommandFormat DataFormat => (CommandFormat)Buffer[3];

		protected byte[] Data
		{
			get
			{
				switch (DataFormat)
				{
					case CommandFormat.OneByteData:
						return new[] { Buffer[4] };
					case CommandFormat.FourByteData:
						return new[] { Buffer[4], Buffer[5], Buffer[6], Buffer[7] };
					case CommandFormat.Undefined:
					case CommandFormat.LED:
						return new byte[0];
					default:
						return new byte[0];
				}
			}
		}

		public override bool Equals(object obj)
		{
			var other = obj as ReceivedCommandData;

			if (other == null || Buffer == null || other.Buffer == null || Buffer.Length != other.Buffer.Length)
			{
				return false;
			}

			return !Buffer.Where((t, i) => t != other.Buffer[i]).Any();
		}

		public override int GetHashCode()
		{
			return (Buffer != null ? Buffer.Sum(x => x) : 0);
		}

		public override string ToString()
		{
			return string.Join("", Buffer.Select(b => b.ToString("x2")));
		}
	}
}
