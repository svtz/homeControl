using System;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using ThinkingHome.NooLite;

namespace homeControl.NooliteF.Adapters
{
    internal class AdapterWrapper : IDisposable, IMtrfAdapter
    {
        private readonly ILogger _logger;
        
        private MTRFXXAdapter Adapter { get; }
        private readonly object _lock = new object();

        public AdapterWrapper(string portName, ILogger logger)
        {
            Adapter = new MTRFXXAdapter(portName);
            _logger = logger.ForContext<MTRFXXAdapter>();
        }

        protected string DeviceName => typeof(MTRFXXAdapter).ToString();

        public event EventHandler<ReceivedData> ReceiveData;

        
        void IMtrfAdapter.Activate()
        {
            EnsureAdapterConnected();
        }

        void IMtrfAdapter.On(byte channel)
        {
            Adapter.On(channel);
        }

        void IMtrfAdapter.OnF(byte channel, uint? deviceId)
        {
            Adapter.OnF(channel, deviceId);
        }

        void IMtrfAdapter.Off(byte channel)
        {
            Adapter.Off(channel);
        }

        void IMtrfAdapter.OffF(byte channel, uint? deviceId)
        {
            Adapter.OffF(channel, deviceId);
        }

        void IMtrfAdapter.Switch(byte channel)
        {
            Adapter.Switch(channel);
        }

        void IMtrfAdapter.SwitchF(byte channel, uint? deviceId)
        {
            Adapter.SwitchF(channel, deviceId);
        }

        void IMtrfAdapter.SetBrightness(byte channel, byte brightness)
        {
            Adapter.SetBrightness(channel, brightness);
        }

        void IMtrfAdapter.SetBrightnessF(byte channel, byte brightness, uint? deviceId)
        {
            Adapter.SetBrightnessF(channel, brightness, deviceId);
        }


        private void EnsureAdapterConnected()
        {
            if (Adapter.IsOpened)
                return;

            lock (_lock)
            {
                if (Adapter.IsOpened)
                    return;
                
                Adapter.Connect += OnConnect;
                Adapter.Disconnect += OnDisconnect;
                Adapter.ReceiveData += OnReceiveData;
                Adapter.ReceiveMicroclimateData += OnReceiveMicroclimateData;
                Adapter.Error += OnError;
                Adapter.Open();
                Adapter.ExitServiceMode();
            }
        }

        private void OnError(object sender, Exception ex)
        {
            _logger.Error(ex, "Ошибка в работе адаптера!");
        }

        private void OnReceiveMicroclimateData(object sender, MicroclimateData data)
        {
            _logger.Information($"Получены климатические данные: {JsonConvert.SerializeObject(data, Formatting.Indented, new StringEnumConverter())}");
        }

        private void OnReceiveData(object sender, ReceivedData data)
        {
            _logger.Information($"Получены данные: {JsonConvert.SerializeObject(data, Formatting.Indented, new StringEnumConverter())}");
            
            var handler = Interlocked.CompareExchange(ref ReceiveData, null, null);
            handler?.Invoke(this, data);
        }

        private void OnDisconnect(object obj)
        {
            _logger.Warning("Адаптер отключен");
        }

        private void OnConnect(object obj)
        {
            _logger.Warning("Адаптер подключен");
        }

        public virtual void Dispose()
        {
            if (Adapter.IsOpened)
            {
                Adapter.Connect -= OnConnect;
                Adapter.Disconnect -= OnDisconnect;
                Adapter.ReceiveData -= OnReceiveData;
                Adapter.ReceiveMicroclimateData -= OnReceiveMicroclimateData;
                Adapter.Error -= OnError;
            }
            
            Adapter.Dispose();
        }
    }
}