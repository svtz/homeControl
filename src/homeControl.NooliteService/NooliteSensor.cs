using System;
using System.Collections.Generic;
using System.Linq;
using homeControl.Configuration;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Sensors;
using homeControl.NooliteService.Adapters;
using homeControl.NooliteService.Configuration;
using Serilog;
using ThinkingHome.NooLite.ReceivedData;

namespace homeControl.NooliteService
{
    internal sealed class NooliteSensor : IDisposable
    {
        private const byte CommandOn = 2;
        private const byte CommandOff = 0;

        private readonly IEventSender _eventSender;
        private readonly IRX2164Adapter _adapter;
        private readonly ILogger _log;
        private readonly Lazy<IDictionary<byte, NooliteSensorInfo>> _channelToConfig;

        public NooliteSensor(
            IEventSender eventSender,
            IRX2164Adapter adapter,
            INooliteSensorInfoRepository configuration,
            ILogger log)
        {
            Guard.DebugAssertArgumentNotNull(eventSender, nameof(eventSender));
            Guard.DebugAssertArgumentNotNull(adapter, nameof(adapter));
            Guard.DebugAssertArgumentNotNull(configuration, nameof(configuration));

            _eventSender = eventSender;
            _adapter = adapter;
            _log = log;
            _channelToConfig = new Lazy<IDictionary<byte, NooliteSensorInfo>>(() => LoadConfig(configuration));
        }

        public void Activate()
        {
            _adapter.CommandReceived += AdapterOnCommandReceived;
            _adapter.Activate();
            _log.Debug("Noolite sensor started.");
        }

        private static Dictionary<byte, NooliteSensorInfo> LoadConfig(INooliteSensorInfoRepository config)
        {
            Guard.DebugAssertArgumentNotNull(config, nameof(config));

            try
            {
                return config.GetAll().Result.ToDictionary(cfg => cfg.Channel);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidConfigurationException(ex, "Found duplicated Noolite sensor channels in the configuration file.");
            }
        }

        private void AdapterOnCommandReceived(ReceivedCommandData receivedCommandData)
        {
            Guard.DebugAssertArgumentNotNull(receivedCommandData, nameof(receivedCommandData));

            NooliteSensorInfo info;
            if (!_channelToConfig.Value.TryGetValue(receivedCommandData.Channel, out info))
                throw new InvalidConfigurationException($"Could not locate Noolite channel #{receivedCommandData.Channel} in the configuration.");

            switch (receivedCommandData.Cmd)
            {
                case CommandOn:
                    _eventSender.SendEvent(new SensorActivatedEvent(info.SensorId));
                    _log.Information("Noolite sensor activated: {SensorId}", info.SensorId);

                    break;
                case CommandOff:
                    _eventSender.SendEvent(new SensorDeactivatedEvent(info.SensorId));
                    _log.Information("Noolite sensor deactivated: {SensorId}", info.SensorId);

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(receivedCommandData.Cmd));
            }
        }

        public void Dispose()
        {
            _adapter.CommandReceived -= AdapterOnCommandReceived;
        }
    }
}
