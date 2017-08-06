using System;
using ThinkingHome.NooLite.ReceivedData;

namespace homeControl.NooliteService.Adapters
{
    internal interface IRX2164Adapter
    {
        event Action<MicroclimateReceivedCommandData> MicroclimateDataReceived;
        event Action<ReceivedCommandData> CommandReceived;

        void Activate();
    }
}