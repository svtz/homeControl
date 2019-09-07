using System;
using ThinkingHome.NooLite.LibUsb.Common;

namespace ThinkingHome.NooLite.LibUsb
{
    /// <summary>
	/// Class for working wuth device
	/// </summary>
	public class PC11XXAdapter : BaseAdapter
	{
		protected override int ProductId =>  0x05DF;
		protected override Func<string, bool> ProductNameFilter => 
			name => true;

		public void SendLedCommand(
			PC11XXLedCommand cmd,
			byte channel,
			byte levelR = 0,
			byte levelG = 0,
			byte levelB = 0)
		{
			var format = cmd == PC11XXLedCommand.SetLevel ? CommandFormat.FourByteData : CommandFormat.LED;

			SendCommandInternal((byte)cmd, channel, (byte)format, levelR, levelG, levelB);
		}

		public void SendCommand(PC11XXCommand cmd, byte channel, byte level = 0)
		{
			var format = cmd == PC11XXCommand.SetLevel ? CommandFormat.OneByteData : CommandFormat.Undefined;

			SendCommandInternal((byte)cmd, channel, (byte)format, level);
		}

		private void SendCommandInternal(
			byte cmd,
			byte channel,
			byte format,
			byte level0 = 0,
			byte level1 = 0,
			byte level2 = 0)
		{
			var buffer = CreateCommand(
				0x30,
				cmd,
				format,
				0,
				channel,
				level0,
				level1,
				level2
			);
			
			WriteBufferData(buffer);
		}
	}
}
