using System;
using homeControl.Peripherals;
using ThinkingHome.NooLite;

namespace homeControl.Noolite.Adapters
{
    internal sealed class PC11XXAdapterWrapper : AdapterWrapperBase<PC11XXAdapter>, IPC11XXAdapter
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