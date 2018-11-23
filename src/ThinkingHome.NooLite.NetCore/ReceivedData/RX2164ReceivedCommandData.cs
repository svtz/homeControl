namespace ThinkingHome.NooLite.ReceivedData
{
	public class RX2164ReceivedCommandData : ReceivedCommandData
	{
		public RX2164ReceivedCommandData(byte[] source)
			: base(source)
		{
		}

		public int ToggleValue
		{
			get { return Buffer[0] & 0x3f; }
		}
	}
}
