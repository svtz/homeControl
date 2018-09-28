using System;
using System.Threading;
using LibUsbDotNet.Main;
using ThinkingHome.NooLite.ReceivedData;

namespace ThinkingHome.NooLite.Common
{
    public abstract class BaseRxAdapter : BaseAdapter
	{
		private readonly Timer _timer;

		protected override int ProductId => 0x05DC;
		

		#region Lifecycle

		private const int TimerIntervalMilliseconds = 200; 

		protected abstract void TimerElapsed();

		protected BaseRxAdapter()
		{
			_timer = new Timer(TimerIntervalMilliseconds);
			_timer.Elapsed += (s, e) => TimerElapsed();
		}

		public override bool OpenDevice()
		{
			if (!base.OpenDevice())
			{
				return false;
			}

			_timer.Start();
			return true;
		}

		public override void Dispose()
		{
			base.Dispose();
			_timer.Dispose();
		}

		#endregion

		
		#region Events

		public event Action<ReceivedCommandData> CommandReceived;

		protected void OnCommandReceived(ReceivedCommandData obj)
		{
			var handler = Interlocked.CompareExchange(ref CommandReceived, null, null);
			handler?.Invoke(obj);
		}

		#endregion

		
		#region IO

		public void SendCommand(RxCommand cmd, byte channel = 0)
		{
			var buffer = CreateCommand(0, (byte) cmd, channel);
			WriteBufferData(buffer);
		}

		#endregion
	}
}
