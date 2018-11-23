using System;
using System.Threading;
using ThinkingHome.NooLite.Common;
using ThinkingHome.NooLite.ReceivedData;

namespace ThinkingHome.NooLite
{
    public class RX2164Adapter : BaseRxAdapter
    {
		protected override Func<string, bool> ProductNameFilter => 
			name => string.Equals(name, "rx2164", StringComparison.OrdinalIgnoreCase);

		private readonly object _lockObject = new object();

		private RX2164ReceivedCommandData _lastReceivedData;

		protected override void TimerElapsed()
		{
			lock (_lockObject)
			{
				var buf = ReadBufferData();
				var current = new RX2164ReceivedCommandData(buf);
				var prev = _lastReceivedData ?? current;

				// обновляем поле с последней полученной командой
				_lastReceivedData = current;

				// генерируем события
				if (current.ToggleValue != prev.ToggleValue)
				{
					OnCommandReceived(current);

					if (current.Cmd == 21 && current.DataFormat == CommandFormat.FourByteData)
					{
						var data = new MicroclimateReceivedCommandData(current.Buffer);
						OnMicroclimateDataReceived(data);
					}
				}
			}
		}


		#region Events

		public event Action<MicroclimateReceivedCommandData> MicroclimateDataReceived;

		protected virtual void OnMicroclimateDataReceived(MicroclimateReceivedCommandData obj)
		{
			var handler = Interlocked.CompareExchange(ref MicroclimateDataReceived, null, null);
			handler?.Invoke(obj);
		}

		#endregion
	}
}
