using System;
using ThinkingHome.NooLite.Common;
using ThinkingHome.NooLite.ReceivedData;

namespace ThinkingHome.NooLite
{
	public class RX1164Adapter : BaseRxAdapter
	{

		#region fields & properties

		protected override Func<string, bool> ProductNameFilter => 
			name => string.Equals(name, "rx1164", StringComparison.OrdinalIgnoreCase);
		
		private RX1164ReceivedCommandData _lastReceivedData;
		private bool _lastProcessedTogl;						// предыдущее значение TOGL
		private byte _lastProcessedCommand = byte.MaxValue;	// предыдущая команда
		private readonly object _lockObject = new object();

		#endregion

		protected override void TimerElapsed()
		{
			RX1164ReceivedCommandData prev, current;

			lock (_lockObject)
			{
				var buf = ReadBufferData();

				prev = _lastReceivedData;
				_lastReceivedData = current = new RX1164ReceivedCommandData(buf);
				
				if (prev == null)
				{
					prev = current;
					_lastProcessedTogl = current.ToggleFlag;
					_lastProcessedCommand = current.Cmd;
				}
			}

			if (current.Equals(prev) &&
				(current.ToggleFlag != _lastProcessedTogl || current.Cmd != _lastProcessedCommand))
			{
				_lastProcessedTogl = current.ToggleFlag;
				_lastProcessedCommand = current.Cmd;

				OnCommandReceived(current);
			}
		}
	}
}
