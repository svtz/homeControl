using System;
using ThinkingHome.NooLite;
using ThinkingHome.NooLite.LibUsb;
using ThinkingHome.NooLite.LibUsb.ReceivedData;

namespace homeControl.NooliteService.Adapters
{
    internal sealed class RX2164AdapterWrapper : AdapterWrapperBase<RX2164Adapter>, IRX2164Adapter
    {
        public RX2164AdapterWrapper()
        {
            Adapter.CommandReceived += OnCommandReceived;
            Adapter.MicroclimateDataReceived += OnMicroclimateDataReceived;
        }

        public event Action<ReceivedCommandData> CommandReceived;

        public void Activate()
        {
            EnsureAdapterConnected();
        }

        public event Action<MicroclimateReceivedCommandData> MicroclimateDataReceived;

        private void OnCommandReceived(ReceivedCommandData obj)
        {
            CommandReceived?.Invoke(obj);
        }

        private void OnMicroclimateDataReceived(MicroclimateReceivedCommandData obj)
        {
            MicroclimateDataReceived?.Invoke(obj);
        }

        public override void Dispose()
        {
            Adapter.CommandReceived -= OnCommandReceived;
            Adapter.MicroclimateDataReceived -= OnMicroclimateDataReceived;

            base.Dispose();
        }
    }
}