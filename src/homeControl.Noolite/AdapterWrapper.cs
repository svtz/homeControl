using System;
using homeControl.Peripherals;
using ThinkingHome.NooLite;
using ThinkingHome.NooLite.Common;

namespace homeControl.Noolite
{
    internal sealed class AdapterWrapper<TAdapter> : IPC11XXAdapter, IDisposable
        where TAdapter : BaseAdapter, IPC11XXAdapter, IDisposable, new()
    {
        private readonly TAdapter _adapter;

        private string DeviceName => typeof(TAdapter).ToString();

        public AdapterWrapper()
        {
            _adapter = new TAdapter();
        }

        private void EnsureAdapterConnected()
        {
            if (!_adapter.IsConnected)
                throw new DeviceDisconnectedException(DeviceName);

            if (!_adapter.IsOpen)
            {
                if (!_adapter.OpenDevice())
                    throw new DeviceInitializationException(DeviceName, "Could not open the device.");
            }
        }

        public void SendLedCommand(PC11XXLedCommand cmd, byte channel, byte levelR = 0, byte levelG = 0, byte levelB = 0)
        {
            EnsureAdapterConnected();

            try
            {
                _adapter.SendLedCommand(cmd, channel, levelR, levelG, levelB);
            }
            catch (Exception ex)
            {
                throw new UnknownDeviceException(ex, DeviceName);
            }
        }

        public void SendCommand(PC11XXCommand cmd, byte channel, byte level = 0)
        {
            EnsureAdapterConnected();

            try
            {
                _adapter.SendCommand(cmd, channel, level);
            }
            catch (Exception ex)
            {
                throw new UnknownDeviceException(ex, DeviceName);
            }
        }

        public void Dispose()
        {
            _adapter.Dispose();
        }
    }
}