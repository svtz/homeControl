using System;
using ThinkingHome.NooLite.ReceivedData;

namespace ThinkingHome.NooLite.Common
{
    public abstract class BaseRxAdapter : BaseAdapter
	{
		protected readonly object lockObject = new object();
		private readonly Timer timer;

		public override int ProductId
		{
			get { return 0x05DC; }
		}

		#region Lifecycle

		protected abstract void TimerElapsed();

		protected BaseRxAdapter()
		{
			timer = new Timer(200);
			timer.Elapsed += (s, e) => TimerElapsed();
		}

		public override bool OpenDevice()
		{
			if (!base.OpenDevice())
			{
				return false;
			}

			timer.Start();
			return true;
		}

		public override void Dispose()
		{
			base.Dispose();
			timer.Dispose();
		}

		#endregion

		#region Events

		public event Action<ReceivedCommandData> CommandReceived;

		protected virtual void OnCommandReceived(ReceivedCommandData obj)
		{
			var handler = CommandReceived;

			if (handler != null)
			{
				handler(obj);
			}
		}

		#endregion

		#region IO

		public byte[] ReadBufferData()
		{
			byte[] buf;
			device.ReadFeatureData(out buf);

			return buf;
		}

		public void SendCommand(RxCommand cmd, byte channel = 0)
		{
			var data = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

			data[1] = (byte)cmd;
			data[2] = channel;

			device.WriteFeatureData(data);
			System.Threading.Thread.Sleep(200);
		}

		#endregion
	}
}
