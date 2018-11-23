namespace ThinkingHome.NooLite.ReceivedData
{
	public class RX1164ReceivedCommandData : ReceivedCommandData
	{
		public RX1164ReceivedCommandData(byte[] source)
			: base(source)
		{
		}

		public bool ToggleFlag => (Buffer[0] & 0x80) > 0;
	}
}
