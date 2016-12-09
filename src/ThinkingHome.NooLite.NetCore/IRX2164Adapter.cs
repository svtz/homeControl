using System;
using ThinkingHome.NooLite.ReceivedData;

namespace ThinkingHome.NooLite
{
    public interface IRX2164Adapter
    {
        event Action<MicroclimateReceivedCommandData> MicroclimateDataReceived;
        event Action<ReceivedCommandData> CommandReceived;
    }
}