using System;
using System.Threading.Tasks;
using homeControl.Configuration;
using homeControl.Domain;
using homeControl.NooliteF.Adapters;
using homeControl.NooliteF.Configuration;

namespace homeControl.NooliteF.SwitchController
{
    internal sealed class NooliteFSwitchController : ISwitchController
    {
        private readonly INooliteFSwitchInfoRepository _configurationRepository;
        private readonly IMtrfAdapter _adapter;
        private readonly NooliteFSwitchesStatusHolder _statusHolder;

        public NooliteFSwitchController(
            INooliteFSwitchInfoRepository configurationRepository,
            IMtrfAdapter adapter,
            NooliteFSwitchesStatusHolder statusHolder)
        {
            Guard.DebugAssertArgumentNotNull(configurationRepository, nameof(configurationRepository));
            Guard.DebugAssertArgumentNotNull(adapter, nameof(adapter));

            _configurationRepository = configurationRepository;
            _adapter = adapter;
            _statusHolder = statusHolder;
        }

        private bool _initializeCalled = false;
        public async Task InitializeState()
        {
            if (_initializeCalled)
                throw new InvalidOperationException();

            _initializeCalled = true;

            foreach (var switchInfo in await _configurationRepository.GetAll())
            {
                if (switchInfo.UseF)
                {
                    RequestStatus(switchInfo.SwitchId);
                }
                else
                {
                    TurnOff(switchInfo.SwitchId);
                }
            }
        }
        
        public bool CanHandleSwitch(SwitchId switchId)
        {
            Guard.DebugAssertArgumentNotNull(switchId, nameof(switchId));

            return _configurationRepository.ContainsConfig(switchId).Result;
        }

        public void TurnOn(SwitchId switchId)
        {
            Guard.DebugAssertArgumentNotNull(switchId, nameof(switchId));
            Guard.DebugAssertArgument(CanHandleSwitch(switchId), nameof(switchId));

            var config = _configurationRepository.GetConfig(switchId).Result;

            var currentStatus = _statusHolder.GetStatus(config.Channel);
            if (currentStatus != null && currentStatus.Value.isOn)
            {
                return;
            }
            
            if (config.UseF)
                _adapter.OnF(config.Channel);
            else
            {
                _adapter.On(config.Channel);
            }
        }

        public void TurnOff(SwitchId switchId)
        {
            Guard.DebugAssertArgumentNotNull(switchId, nameof(switchId));
            Guard.DebugAssertArgument(CanHandleSwitch(switchId), nameof(switchId));

            var config = _configurationRepository.GetConfig(switchId).Result;
            
            var currentStatus = _statusHolder.GetStatus(config.Channel);
            if (currentStatus != null && !currentStatus.Value.isOn)
            {
                return;
            }
            
            if (config.UseF)
                _adapter.OffF(config.Channel);
            else
            {
                _adapter.Off(config.Channel);
            }
        }

        public void SetPower(SwitchId switchId, double power)
        {
            Guard.DebugAssertArgumentNotNull(switchId, nameof(switchId));
            Guard.DebugAssertArgument(power >= 0 && power <= 1, nameof(switchId));
            Guard.DebugAssertArgument(CanHandleSwitch(switchId), nameof(switchId));

            var config = _configurationRepository.GetConfig(switchId).Result;
            if (config.FullPowerLevel <= config.ZeroPowerLevel)
            {
                throw new InvalidConfigurationException($"Invalid configuration for switch {switchId}. FullPowerLevel should be greater then ZeroPowerLevel.");
            }

            var level = Convert.ToByte(config.ZeroPowerLevel + (config.FullPowerLevel - config.ZeroPowerLevel) * power);
            
            var currentStatus = _statusHolder.GetStatus(config.Channel);
            if (currentStatus != null && currentStatus.Value.power == level)
            {
                return;
            }
            
            if (config.UseF)
                _adapter.SetBrightnessF(config.Channel, level);
            else
            {
                _adapter.SetBrightness(config.Channel, level);
            }
        }

        public void RequestStatus(SwitchId switchId)
        {
            Guard.DebugAssertArgumentNotNull(switchId, nameof(switchId));

            var config = _configurationRepository.GetConfig(switchId).Result;
            Guard.DebugAssert(config.UseF, "Switch must be nooliteF to request its status");
            
            _adapter.ReadState(config.Channel);
        }
    }
}
