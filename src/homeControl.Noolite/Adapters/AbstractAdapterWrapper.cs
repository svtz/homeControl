using System;
using homeControl.Peripherals;
using ThinkingHome.NooLite.Common;

namespace homeControl.Noolite.Adapters
{
    internal class AdapterWrapperBase<TAdapter> : IDisposable
        where TAdapter : BaseAdapter, IDisposable, new()
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

        public virtual void Dispose()
        {
            Adapter.Dispose();
        }
    }
}