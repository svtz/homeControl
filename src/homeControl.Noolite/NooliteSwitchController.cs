using homeControl.Configuration.Switches;
using homeControl.Noolite.Adapters;
using homeControl.Noolite.Configuration;
using homeControl.Peripherals;
using ThinkingHome.NooLite;

namespace homeControl.Noolite
{
    internal class NooliteSwitchController : ISwitchController
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
            ExecuteImpl(switchId, PC11XXCommand.On);
        }

        public void TurnOff(SwitchId switchId)
        {
            ExecuteImpl(switchId, PC11XXCommand.Off);
        }

        private void ExecuteImpl(SwitchId switchId, PC11XXCommand command)
        {
            Guard.DebugAssertArgumentNotNull(switchId, nameof(switchId));
            Guard.DebugAssertArgument(CanHandleSwitch(switchId), nameof(switchId));

            var config = _configurationRepository.GetConfig<NooliteSwitchConfig>(switchId);
            _adapter.SendCommand(command, config.Channel);
        }
    }
}
