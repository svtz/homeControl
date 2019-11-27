using System;
using ThinkingHome.NooLite;

namespace homeControl.NooliteF.Adapters
{
    internal interface IMtrfAdapter
    {
        event EventHandler<ReceivedData> ReceiveData;
        void Activate();

        void On(byte channel);

        void OnF(byte channel, uint? deviceId = null);

        void Off(byte channel);

        void OffF(byte channel, uint? deviceId = null);

        void Switch(byte channel);

        void SwitchF(byte channel, uint? deviceId = null);

        void SetBrightness(byte channel, byte brightness);

        void SetBrightnessF(byte channel, byte brightness, uint? deviceId = null);
    }
}