using System;
using homeControl.NooliteService.Diagnostics;
using ThinkingHome.NooLite.Common;

namespace homeControl.NooliteService.Adapters
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
            if (!Adapter.IsOpen)
            {
                if (!Adapter.OpenDevice())
                {
                    throw new DeviceInitializationException(DeviceName, "Could not open the device.");
                }
            }
        }

        public virtual void Dispose()
        {
            Adapter.Dispose();
        }
    }
}