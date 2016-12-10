using System;
using homeControl.Configuration;
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

        public void TurnOn(Guid switchId)
        {
            Guard.DebugAssertArgumentNotDefault(switchId, nameof(switchId));

            ExecuteImpl(switchId, PC11XXCommand.On);
        }

        public void TurnOff(Guid switchId)
        {
            Guard.DebugAssertArgumentNotDefault(switchId, nameof(switchId));

            ExecuteImpl(switchId, PC11XXCommand.Off);
        }

        private void ExecuteImpl(Guid switchId, PC11XXCommand command)
        {
            var config = _configurationRepository.GetSwicthConfig<NooliteSwitchConfig>(switchId);
            _adapter.SendCommand(command, config.Channel);
        }
    }
}
