using System;
using System.Collections.Generic;
using System.Linq;
using homeControl.Configuration;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Sensors;
using homeControl.NooliteF.Adapters;
using homeControl.NooliteF.Configuration;
using Serilog;
using ThinkingHome.NooLite;
using ThinkingHome.NooLite.Internal;

namespace homeControl.NooliteF
{
    internal sealed class NooliteFSensor : IDisposable
    {
        private readonly IEventSender _eventSender;
        private readonly IMtrfAdapter _adapter;
        private readonly ILogger _log;
        private readonly Lazy<IDictionary<int, NooliteFSensorInfo>> _channelToConfig;

        public NooliteFSensor(
            IEventSender eventSender,
            IMtrfAdapter adapter,
            INooliteFSensorInfoRepository configuration,
            ILogger log)
        {
            Guard.DebugAssertArgumentNotNull(eventSender, nameof(eventSender));
            Guard.DebugAssertArgumentNotNull(adapter, nameof(adapter));
            Guard.DebugAssertArgumentNotNull(configuration, nameof(configuration));

            _eventSender = eventSender;
            _adapter = adapter;
            _log = log;
            _channelToConfig = new Lazy<IDictionary<int, NooliteFSensorInfo>>(() => LoadConfig(configuration));
        }

        public void Activate()
        {
            _adapter.ReceiveData += AdapterOnReceiveData;
            _adapter.Activate();
            _log.Debug("Noolite sensor started.");
        }

        private static Dictionary<int, NooliteFSensorInfo> LoadConfig(INooliteFSensorInfoRepository config)
        {
            Guard.DebugAssertArgumentNotNull(config, nameof(config));

            try
            {
                return config.GetAll().Result.ToDictionary(cfg => (int)cfg.Channel);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidConfigurationException(ex, "Found duplicated Noolite.F sensor channels in the configuration file.");
            }
        }

        private void AdapterOnReceiveData(object sender, ReceivedData receivedData)
        {
            Guard.DebugAssertArgumentNotNull(receivedData, nameof(receivedData));

            if (!_channelToConfig.Value.TryGetValue(receivedData.Channel, out var info))
                throw new InvalidConfigurationException($"Could not locate Noolite channel #{receivedData.Channel} in the configuration.");

            switch (receivedData.Command)
            {
                case MTRFXXCommand.On:
                    _eventSender.SendEvent(new SensorActivatedEvent(info.SensorId));
                    _log.Information("Noolite.F sensor activated: {SensorId}", info.SensorId);

                    break;
                case MTRFXXCommand.Off:
                    _eventSender.SendEvent(new SensorDeactivatedEvent(info.SensorId));
                    _log.Information("Noolite.F sensor deactivated: {SensorId}", info.SensorId);

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(receivedData.Command));
            }
        }

        public void Dispose()
        {
            _adapter.ReceiveData -= AdapterOnReceiveData;
        }
    }
}
