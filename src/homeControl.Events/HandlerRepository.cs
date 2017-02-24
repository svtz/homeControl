using System;
using System.Linq;
using homeControl.Configuration.Bindings;
using homeControl.Configuration.Switches;
using homeControl.Core;
using homeControl.Events.Switches;
using homeControl.Peripherals;

namespace homeControl.Events
{
    internal sealed class HandlerRepository : IHandlerRepository
    {
        private readonly ISwitchConfigurationRepository _switchConfigurationRepository;
        private readonly ISwitchToSensorBindingsRepository _switchToSensorBindingsRepository;
        private readonly ISwitchControllerSelector _switchController;
        private readonly EventPublisherAccessor _eventPublisherAccessor;

        public IHandler[] GetHandlers()
        {
            return _handlerLazy.Value;
        }

        private readonly Lazy<IHandler[]> _handlerLazy;

        public HandlerRepository(
            ISwitchConfigurationRepository switchConfigurationRepository,
            ISwitchToSensorBindingsRepository switchToSensorBindingsRepository,
            ISwitchControllerSelector switchController,
            EventPublisherAccessor eventPublisherAccessor)
        {
            _switchConfigurationRepository = switchConfigurationRepository;
            _switchToSensorBindingsRepository = switchToSensorBindingsRepository;
            _switchController = switchController;
            _eventPublisherAccessor = eventPublisherAccessor;

            _handlerLazy = new Lazy<IHandler[]>(CreateHandlers);
        }

        private IHandler[] CreateHandlers()
        {
            var switchHandlers = _switchConfigurationRepository.GetAll()
                .Select(@switch => new SwitchEventHandler(_switchController)
                {
                    SwitchId = @switch.SwitchId
                });

            var bindings = _switchToSensorBindingsRepository.GetAll()
                .Select(binding => new SwitchToSensorBinderHandler(_eventPublisherAccessor.GetEventPublisher())
                {
                    SensorId = binding.SensorId,
                    SwitchId = binding.SwitchId
                });

            return switchHandlers.Concat<IHandler>(bindings).ToArray();
        }
    }
}
