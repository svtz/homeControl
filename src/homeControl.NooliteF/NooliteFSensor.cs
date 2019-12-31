using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using homeControl.Configuration;
using homeControl.Domain;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Sensors;
using homeControl.Domain.Events.Switches;
using homeControl.NooliteF.Adapters;
using homeControl.NooliteF.Configuration;
using homeControl.NooliteF.SwitchController;
using JetBrains.Annotations;
using Serilog;
using ThinkingHome.NooLite;
using ThinkingHome.NooLite.Internal;

namespace homeControl.NooliteF
{
    internal sealed class NooliteFSensor : IDisposable
    {
        private readonly IEventSender _eventSender;
        private readonly IMtrfAdapter _adapter;
        private readonly NooliteFSwitchesStatusHolder _statusHolder;
        private readonly ILogger _log;
        private readonly Lazy<IDictionary<int, AbstractNooliteFSensorInfo>> _rxChannelToSensorConfig;
        private readonly Lazy<IDictionary<int, NooliteFSwitchInfo>> _rxChannelToSwitchConfig;
        private readonly Lazy<IDictionary<int, NooliteFSwitchInfo>> _txChannelToSwitchConfig;

        public NooliteFSensor(
            IEventSender eventSender,
            IMtrfAdapter adapter,
            INooliteFSensorInfoRepository sensorRepository,
            INooliteFSwitchInfoRepository switchInfoRepository,
            NooliteFSwitchesStatusHolder statusHolder,
            ILogger log)
        {
            Guard.DebugAssertArgumentNotNull(eventSender, nameof(eventSender));
            Guard.DebugAssertArgumentNotNull(adapter, nameof(adapter));
            Guard.DebugAssertArgumentNotNull(sensorRepository, nameof(sensorRepository));

            _eventSender = eventSender;
            _adapter = adapter;
            _statusHolder = statusHolder;
            _log = log;
            _rxChannelToSensorConfig = new Lazy<IDictionary<int, AbstractNooliteFSensorInfo>>(() => LoadSensorConfig(sensorRepository));
            _rxChannelToSwitchConfig = new Lazy<IDictionary<int, NooliteFSwitchInfo>>(() => LoadRxSwitchConfig(switchInfoRepository));
            _txChannelToSwitchConfig = new Lazy<IDictionary<int, NooliteFSwitchInfo>>(() => LoadTxSwitchConfig(switchInfoRepository));
        }

        public void Activate()
        {
            _adapter.ReceiveData += AdapterOnReceiveData;
            _adapter.Activate();
            _log.Debug("Noolite sensor started.");
        }

        private static Dictionary<int, AbstractNooliteFSensorInfo> LoadSensorConfig(INooliteFSensorInfoRepository repository)
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
        
        private static Dictionary<int, NooliteFSwitchInfo> LoadTxSwitchConfig(INooliteFSwitchInfoRepository repository)
        {
            Guard.DebugAssertArgumentNotNull(repository, nameof(repository));

            try
            {
                return repository.GetAll().Result.ToDictionary(cfg => (int)cfg.Channel);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidConfigurationException(ex, "Found duplicated Noolite.F switch channels in the configuration file.");
            }
        }
        
        private IDictionary<int, NooliteFSwitchInfo> LoadRxSwitchConfig(INooliteFSwitchInfoRepository switchInfoRepository)
        {
            Guard.DebugAssertArgumentNotNull(switchInfoRepository, nameof(switchInfoRepository));

            var configs = switchInfoRepository.GetAll().Result;
            try
            {
                return
                    configs
                        .Where(config => config.StatusChannelIds != null)
                        .SelectMany(config => config
                            .StatusChannelIds
                            .Select(channel => new {Channel = (int) channel, Config = config}))
                        .Where(pair => pair != null)
                        .ToDictionary(pair => pair.Channel, pair => pair.Config);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidConfigurationException(ex, "Found duplicated Noolite.F switch status channels in the configuration file.");
            }
        }
        
        private void AdapterOnReceiveData(object sender, ReceivedData receivedData)
        {
            Guard.DebugAssertArgumentNotNull(receivedData, nameof(receivedData));

            NooliteFSwitchInfo switchInfo;
            
            switch (receivedData.Mode, receivedData.Command, receivedData.Result)
            {
                case (MTRFXXMode.Service, _, _):
                    // this happens at startup when block exits service mode
                    break;

                case (MTRFXXMode.RX, MTRFXXCommand.On, ResultCode.Success):
                case (MTRFXXMode.RXF, MTRFXXCommand.On, ResultCode.Success):
                case (MTRFXXMode.RX, MTRFXXCommand.TemporarySwitchOn, ResultCode.Success):
                case (MTRFXXMode.RXF, MTRFXXCommand.TemporarySwitchOn, ResultCode.Success):
                    switchInfo = TryGetSwitchInfo(receivedData);
                    if (switchInfo != null)
                    {
                        _statusHolder.SetStatus(switchInfo.Channel, 0 /* todo */, true);
                        _eventSender.SendEvent(new TurnSwitchOnEvent(switchInfo.SwitchId));
                        _log.Information("Noolite.F switch on: {SwitchId}", switchInfo.SwitchId);
                    }

                    var onSensorInfo = TryGetSensorInfo<OnOffNooliteFSensorInfo>(receivedData);
                    if (onSensorInfo != null)
                    {
                        _eventSender.SendEvent(new SensorActivatedEvent(onSensorInfo.SensorId));
                        _log.Information("Noolite.F sensor activated: {SensorId}", onSensorInfo.SensorId);
                    }
                    else if (switchInfo == null)
                    {
                        ThrowSensorNotFound(receivedData);
                    }

                    break;
                
                case (MTRFXXMode.RX, MTRFXXCommand.Off, ResultCode.Success):
                case (MTRFXXMode.RXF, MTRFXXCommand.Off, ResultCode.Success):
                    switchInfo = TryGetSwitchInfo(receivedData);
                    if (switchInfo != null)
                    {
                        _statusHolder.SetStatus(switchInfo.Channel, 0 /* todo */, false);
                        _eventSender.SendEvent(new TurnSwitchOffEvent(switchInfo.SwitchId));
                        _log.Information("Noolite.F switch off: {SwitchId}", switchInfo.SwitchId);
                    }

                    var offSensorInfo = TryGetSensorInfo<OnOffNooliteFSensorInfo>(receivedData);
                    if (offSensorInfo != null)
                    {
                        _eventSender.SendEvent(new SensorDeactivatedEvent(offSensorInfo.SensorId));
                        _log.Information("Noolite.F sensor deactivated: {SensorId}", offSensorInfo.SensorId);
                    }
                    else if (switchInfo == null)
                    {
                        ThrowSensorNotFound(receivedData);
                    }

                    break;
                    
                case (MTRFXXMode.TXF, MTRFXXCommand.On, ResultCode.NoResponse):
                case (MTRFXXMode.TXF, MTRFXXCommand.Off, ResultCode.NoResponse):
                    //todo noResponse event?
                    break;
                
                case (MTRFXXMode.TXF, MTRFXXCommand.SendState, ResultCode.Success) when receivedData.Data3 == 1:
                case (MTRFXXMode.TX, MTRFXXCommand.On, ResultCode.Success):
                    switchInfo = TryGetSwitchInfo(receivedData);
                    _statusHolder.SetStatus(switchInfo.Channel, 0 /* todo */, true);
                    _eventSender.SendEvent(new TurnSwitchOnEvent(switchInfo.SwitchId));
                    _log.Information("Noolite.F switch on: {SwitchId}", switchInfo.SwitchId);
                    break;
                
                case (MTRFXXMode.TXF, MTRFXXCommand.SendState, ResultCode.Success) when receivedData.Data3 == 0:
                case (MTRFXXMode.TX, MTRFXXCommand.Off, ResultCode.Success):
                    switchInfo = TryGetSwitchInfo(receivedData);
                    _statusHolder.SetStatus(switchInfo.Channel, 0 /* todo */, false);
                    _eventSender.SendEvent(new TurnSwitchOffEvent(switchInfo.SwitchId));
                    _log.Information("Noolite.F switch off: {SwitchId}", switchInfo.SwitchId);
                    break;
                
                case (MTRFXXMode.RX, MTRFXXCommand.MicroclimateData, ResultCode.Success) when receivedData is MicroclimateData microclimateData:
                    var temperatureSensor = GetSensorInfo<TemperatureNooliteFSensorInfo>(receivedData);
                    _eventSender.SendEvent(new SensorValueEvent(temperatureSensor.TemperatureSensorId, microclimateData.Temperature));
                    if (microclimateData.Humidity.HasValue &&
                        temperatureSensor is TemperatureAndHumidityNooliteFSensorInfo humiditySensor)
                    {
                        _eventSender.SendEvent(new SensorValueEvent(humiditySensor.HumiditySensorId, microclimateData.Humidity.Value));
                    }
                    
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(receivedData.Command));
            }
        }

        private NooliteFSwitchInfo TryGetSwitchInfo(ReceivedData receivedData)
        {
            NooliteFSwitchInfo info;
            switch (receivedData.Mode)
            {
                case MTRFXXMode.TX:
                case MTRFXXMode.TXF:
                    if (!_txChannelToSwitchConfig.Value.TryGetValue(receivedData.Channel, out info))
                        throw new InvalidConfigurationException($"Could not locate Noolite channel #{receivedData.Channel} in the switches configuration.");
                    return info;

                case MTRFXXMode.RX:
                case MTRFXXMode.RXF:
                    if (!_rxChannelToSwitchConfig.Value.TryGetValue(receivedData.Channel, out info))
                        return null;
                    return info;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        [ContractAnnotation("=> halt")]
        private void ThrowSensorNotFound(ReceivedData receivedData) =>
            throw new InvalidConfigurationException($"Could not locate Noolite channel #{receivedData.Channel} in the sensors configuration.");
        
        private TInfo GetSensorInfo<TInfo>(ReceivedData receivedData) where TInfo : AbstractNooliteFSensorInfo
        {
            var info = TryGetSensorInfo<TInfo>(receivedData);
            if (info == null)
                ThrowSensorNotFound(receivedData);
            
            return info;
        }
        
        private TInfo TryGetSensorInfo<TInfo>(ReceivedData receivedData) where TInfo : AbstractNooliteFSensorInfo
        {
            if (!_rxChannelToSensorConfig.Value.TryGetValue(receivedData.Channel, out var info))
                return null;

            if (!(info is TInfo typedInfo))
                throw new InvalidConfigurationException($"Noolite sensor with channel #{receivedData.Channel} has invalid type {info.GetType()}. Expected type: {typeof(TInfo)}");
            
            return typedInfo;
        }

        public void Dispose()
        {
            _adapter.ReceiveData -= AdapterOnReceiveData;
        }
    }
}
