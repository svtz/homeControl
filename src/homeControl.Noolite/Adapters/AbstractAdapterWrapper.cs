using System;
using homeControl.Peripherals;
using ThinkingHome.NooLite;
using ThinkingHome.NooLite.Common;

namespace homeControl.Noolite
{
    internal class AdapterWrapperBase<TAdapter> : IDisposable where TAdapter : BaseAdapter, IPC11XXAdapter, IDisposable, new()
    {
        protected TAdapter Adapter { get; }

        protected AdapterWrapperBase()
        {
            Adapter = new TAdapter();
        }

        protected string DeviceName => typeof(TAdapter).ToString();

        protected void EnsureAdapterConnected()
        {
            if (!Adapter.IsConnected)
                throw new DeviceDisconnectedException(DeviceName);

            if (!Adapter.IsOpen)
            {
                if (!Adapter.OpenDevice())
                    throw new DeviceInitializationException(DeviceName, "Could not open the device.");
            }
        }

        public void Dispose()
        {
            Adapter.Dispose();
        }
    }
}