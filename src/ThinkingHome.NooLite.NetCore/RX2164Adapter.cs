using System;
using System.Linq;
using HidLibrary;
using ThinkingHome.NooLite.Common;
using ThinkingHome.NooLite.ReceivedData;

namespace ThinkingHome.NooLite
{
    public class RX2164Adapter : BaseRxAdapter, IRX2164Adapter
    {
		protected RX2164ReceivedCommandData lastReceivedData;

		protected override HidDevice SelectDevice()
		{
			var hidDevice = HidDevices
				.Enumerate(VendorId, ProductId)
				.FirstOrDefault(a => StringComparer.OrdinalIgnoreCase.Equals(GetProductString(a), "rx2164"));

			return hidDevice;
		}

		protected override void TimerElapsed()
		{
			lock (lockObject)
			{
				var buf = ReadBufferData();
				var current = new RX2164ReceivedCommandData(buf);
				var prev = lastReceivedData ?? current;

				// обновляем поле с последней полученной командой
				lastReceivedData = current;

				// генерируем события
				if (current.ToggleValue != prev.ToggleValue)
				{
					OnCommandReceived(current);

					if (current.Cmd == 21 && current.DataFormat == CommandFormat.FourByteData)
					{
						var data = new MicroclimateReceivedCommandData(current.buf);
						OnMicroclimateDataReceived(data);
					}
				}
			}
		}


		#region Events

		public event Action<MicroclimateReceivedCommandData> MicroclimateDataReceived;

		protected virtual void OnMicroclimateDataReceived(MicroclimateReceivedCommandData obj)
		{
			var handler = MicroclimateDataReceived;

			if (handler != null)
			{
				handler(obj);
			}
		}

		#endregion
	}
}
