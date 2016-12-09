using System;
using ThinkingHome.NooLite;
using ThinkingHome.NooLite.Common;
using ThinkingHome.NooLite.ReceivedData;

namespace homeControl.Noolite.Adapters
{
    internal sealed class ReceiverAdapterWrapper<TAdapter> : AdapterWrapperBase<TAdapter>, IRX2164Adapter
        where TAdapter : BaseAdapter, IRX2164Adapter, IDisposable, new()
    {
        public ReceiverAdapterWrapper()
        {
            Adapter.CommandReceived += OnCommandReceived;
            Adapter.MicroclimateDataReceived += OnMicroclimateDataReceived;
        }

        public event Action<ReceivedCommandData> CommandReceived;
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