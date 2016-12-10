using System;
using System.Linq;
using homeControl.Configuration;
using homeControl.Core;
using homeControl.Peripherals;

namespace homeControl.Events.Switches
{
    internal sealed class HandlerFactory : IHandlerFactory
    {
        private readonly ISwitchConfigurationRepository _switchConfigurationRepository;
        private readonly ISwitchControllerSelector _switchController;

        public IHandler[] CreateHandlers()
        {
            return _handlerLazy.Value;
        }

        private readonly Lazy<IHandler[]> _handlerLazy;

        public HandlerFactory(
            ISwitchConfigurationRepository switchConfigurationRepository,
            ISwitchControllerSelector switchController)
        {
            _switchConfigurationRepository = switchConfigurationRepository;
            _switchController = switchController;

            _handlerLazy = new Lazy<IHandler[]>(() =>
                    _switchConfigurationRepository.GetAllIds()
                        .Select(id => new SwitchEventHandler(_switchController) {SwitchId = id})
                        .ToArray<IHandler>()
            );
        }
    }
}
