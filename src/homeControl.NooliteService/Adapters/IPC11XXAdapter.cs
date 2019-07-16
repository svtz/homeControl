using ThinkingHome.NooLite;
using ThinkingHome.NooLite.LibUsb;

namespace homeControl.NooliteService.Adapters
{
    internal interface IPC11XXAdapter
    {
        void SendLedCommand(
            PC11XXLedCommand cmd,
            byte channel,
            byte levelR = 0,
            byte levelG = 0,
            byte levelB = 0);

        void SendCommand(PC11XXCommand cmd, byte channel, byte level = 0);
    }
}