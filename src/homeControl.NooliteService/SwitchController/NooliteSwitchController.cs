using System;
using homeControl.Configuration;
using homeControl.Configuration.Switches;
using homeControl.Noolite.Adapters;
using homeControl.NooliteService.Configuration;
using homeControl.Peripherals;
using ThinkingHome.NooLite;

namespace homeControl.NooliteService
{
    internal sealed class NooliteSwitchController : ISwitchController
    {
        private readonly ISwitchConfigurationRepository _configurationRepository;
        private readonly IPC11XXAdapter _adapter;

        public NooliteSwitchController(
            ISwitchConfigurationRepository configurationRepository,
            IPC11XXAdapter adapter)
        {
            Guard.DebugAssertArgumentNotNull(configurationRepository, nameof(configurationRepository));
            Guard.DebugAssertArgumentNotNull(adapter, nameof(adapter));

            _configurationRepository = configurationRepository;
            _adapter = adapter;
        }

        public bool CanHandleSwitch(SwitchId switchId)
        {
            Guard.DebugAssertArgumentNotNull(switchId, nameof(switchId));

            return _configurationRepository.ContainsConfig<NooliteSwitchConfig>(switchId);
        }

        public void TurnOn(SwitchId switchId)
        {
            Guard.DebugAssertArgumentNotNull(switchId, nameof(switchId));
            Guard.DebugAssertArgument(CanHandleSwitch(switchId), nameof(switchId));

            var config = _configurationRepository.GetConfig<NooliteSwitchConfig>(switchId);
            _adapter.SendCommand(PC11XXCommand.On, config.Channel);
        }

        public void TurnOff(SwitchId switchId)
        {
            Guard.DebugAssertArgumentNotNull(switchId, nameof(switchId));
            Guard.DebugAssertArgument(CanHandleSwitch(switchId), nameof(switchId));

            var config = _configurationRepository.GetConfig<NooliteSwitchConfig>(switchId);
            _adapter.SendCommand(PC11XXCommand.Off, config.Channel);
        }

        public void SetPower(SwitchId switchId, double power)
        {
            Guard.DebugAssertArgumentNotNull(switchId, nameof(switchId));
            Guard.DebugAssertArgument(power >= 0 && power <= 1, nameof(switchId));
            Guard.DebugAssertArgument(CanHandleSwitch(switchId), nameof(switchId));

            var config = _configurationRepository.GetConfig<NooliteSwitchConfig>(switchId);
            if (config.FullPowerLevel <= config.ZeroPowerLevel)
            {
                throw new InvalidConfigurationException($"Invalid configuration for switch {switchId}. FullPowerLevel should be greater then ZeroPowerLevel.");
            }

            var level = Convert.ToByte(config.ZeroPowerLevel + (config.FullPowerLevel - config.ZeroPowerLevel) * power);
            _adapter.SendCommand(PC11XXCommand.SetLevel, config.Channel, level);
        }
    }
}
