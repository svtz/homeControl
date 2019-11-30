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
        private readonly Lazy<IDictionary<int, NooliteFSensorInfo>> _channelToSensorConfig;
        private readonly Lazy<IDictionary<int, NooliteFSwitchInfo>> _channelToSwitchConfig;

        public NooliteFSensor(
            IEventSender eventSender,
            IMtrfAdapter adapter,
            INooliteFSensorInfoRepository sensorRepository,
            INooliteFSwitchInfoRepository switchRepository,
            ILogger log)
        {
            Guard.DebugAssertArgumentNotNull(eventSender, nameof(eventSender));
            Guard.DebugAssertArgumentNotNull(adapter, nameof(adapter));
            Guard.DebugAssertArgumentNotNull(sensorRepository, nameof(sensorRepository));
            Guard.DebugAssertArgumentNotNull(switchRepository, nameof(switchRepository));

            _eventSender = eventSender;
            _adapter = adapter;
            _log = log;
            _channelToSensorConfig = new Lazy<IDictionary<int, NooliteFSensorInfo>>(() => LoadSensorConfig(sensorRepository));
            _channelToSwitchConfig = new Lazy<IDictionary<int, NooliteFSwitchInfo>>(() => LoadSwitchConfig(switchRepository));
        }

        public void Activate()
        {
            _adapter.ReceiveData += AdapterOnReceiveData;
            _adapter.Activate();
            _log.Debug("Noolite sensor started.");
        }

        private static Dictionary<int, NooliteFSensorInfo> LoadSensorConfig(INooliteFSensorInfoRepository repository)
        {
            Guard.DebugAssertArgumentNotNull(repository, nameof(repository));

            try
            {
                return repository.GetAll().Result.ToDictionary(cfg => (int)cfg.Channel);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidConfigurationException(ex, "Found duplicated Noolite.F sensor channels in the configuration file.");
            }
        }
        
        private static Dictionary<int, NooliteFSwitchInfo> LoadSwitchConfig(INooliteFSwitchInfoRepository repository)
        {
            Guard.DebugAssertArgumentNotNull(repository, nameof(repository));

            try
            {
                return repository.GetAll().Result.ToDictionary(cfg => (int)cfg.Channel);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidConfigurationException(ex, "Found duplicated Noolite.F sensor channels in the configuration file.");
            }
        }

        private void AdapterOnReceiveData(object sender, ReceivedData receivedData)
        {
            Guard.DebugAssertArgumentNotNull(receivedData, nameof(receivedData));

            switch (receivedData.Command)
            {
                case MTRFXXCommand.On:
                    var onSensorInfo = GetSensorInfo(receivedData);
                    _eventSender.SendEvent(new SensorActivatedEvent(onSensorInfo.SensorId));
                    _log.Information("Noolite.F sensor activated: {SensorId}", onSensorInfo.SensorId);
                    break;
                
                case MTRFXXCommand.Off:
                    var offSensorInfo = GetSensorInfo(receivedData);
                    _eventSender.SendEvent(new SensorDeactivatedEvent(offSensorInfo.SensorId));
                    _log.Information("Noolite.F sensor deactivated: {SensorId}", offSensorInfo.SensorId);
                    break;
                    
                case MTRFXXCommand.SendState:
                    if (!_channelToSwitchConfig.Value.TryGetValue(receivedData.Channel, out var switchInfo))
                        throw new InvalidConfigurationException($"Could not locate Noolite channel #{receivedData.Channel} in the switches configuration.");
                    break;
                
                case MTRFXXCommand.MicroclimateData:
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(receivedData.Command));
            }
        }

        private NooliteFSensorInfo GetSensorInfo(ReceivedData receivedData)
        {
            if (!_channelToSensorConfig.Value.TryGetValue(receivedData.Channel, out var info))
                throw new InvalidConfigurationException($"Could not locate Noolite channel #{receivedData.Channel} in the sensors configuration.");

            return info;
        }

        public void Dispose()
        {
            _adapter.ReceiveData -= AdapterOnReceiveData;
        }
    }
}
