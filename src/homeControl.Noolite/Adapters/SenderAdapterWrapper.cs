using System;
using homeControl.Peripherals;
using ThinkingHome.NooLite;
using ThinkingHome.NooLite.Common;

namespace homeControl.Noolite.Adapters
{
    internal sealed class SenderAdapterWrapper<TAdapter> : AdapterWrapperBase<TAdapter>, IPC11XXAdapter
        where TAdapter : BaseAdapter, IPC11XXAdapter, IDisposable, new()
    {
        public void SendLedCommand(PC11XXLedCommand cmd, byte channel, byte levelR = 0, byte levelG = 0, byte levelB = 0)
        {
            EnsureAdapterConnected();

            try
            {
                Adapter.SendLedCommand(cmd, channel, levelR, levelG, levelB);
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
                Adapter.SendCommand(cmd, channel, level);
            }
            catch (Exception ex)
            {
                throw new UnknownDeviceException(ex, DeviceName);
            }
        }
    }
}