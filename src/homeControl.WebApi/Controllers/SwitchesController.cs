using System;
using System.Linq;
using homeControl.ClientApi.Configuration;
using homeControl.ClientServerShared;
using homeControl.ClientServerShared.Dto;
using homeControl.Core;
using homeControl.Events.Bindings;
using homeControl.Events.Switches;

namespace homeControl.ClientApi.Controllers
{
    internal sealed class SwitchesController : ISwitchesApi
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly IClientApiConfigurationRepository _configuration;
        private readonly ISetSwitchValueStrategy[] _setValueStrategies;

        public SwitchesController(IEventPublisher eventPublisher,
            IClientApiConfigurationRepository configuration,
            ISetSwitchValueStrategy[] setSwitchValueStrategies)
        {
            Guard.DebugAssertArgumentNotNull(eventPublisher, nameof(eventPublisher));
            Guard.DebugAssertArgumentNotNull(configuration, nameof(configuration));

            _eventPublisher = eventPublisher;
            _configuration = configuration;

            _setValueStrategies = setSwitchValueStrategies;
        }

        public SwitchDto[] GetDescriptions()
        {
            return _configuration.GetAll().Select(CreateSwitchDto).ToArray();
        }

        private SwitchDto CreateSwitchDto(SwitchApiConfig config)
        {
            Guard.DebugAssertArgumentNotNull(config, nameof(config));

            return new SwitchDto
            {
                Automation = config is AutomatedSwitchApiConfig ? SwitchAutomation.Supported : SwitchAutomation.None,
                Kind = config.Kind,
                Description = config.Description,
                Id = config.ConfigId,
                Name = config.Name
            };
        }

        public bool SetValue(Guid id, object value)
        {
            var config = _configuration.TryGetById(id);
            if (config == null)
            {
                return false;
            }
            var strategy = _setValueStrategies.SingleOrDefault(s => s.CanHandle(config.Kind, value));
            if (strategy == null)
            {
                return false;
            }

            var e1 = strategy.CreateControlEvent(config.SwitchId, value);
            var e2 = strategy.CreateSetPowerEvent(config.SwitchId, value);

            _eventPublisher.PublishEvent(e1);
            _eventPublisher.PublishEvent(e2);

            return true;
        }

        public bool TurnOn(Guid id)
        {
            var config = _configuration.TryGetById(id);
            if (config == null)
            {
                return false;
            }

            _eventPublisher.PublishEvent(new TurnOnEvent(config.SwitchId));

            return true;
        }

        public bool TurnOff(Guid id)
        {
            var config = _configuration.TryGetById(id);
            if (config == null)
            {
                return false;
            }

            _eventPublisher.PublishEvent(new TurnOffEvent(config.SwitchId));

            return true;
        }

        public bool EnableAutomation(Guid id)
        {
            var config = _configuration.TryGetById(id) as AutomatedSwitchApiConfig;
            if (config == null)
            {
                return false;
            }

            _eventPublisher.PublishEvent(new EnableBindingEvent(config.SwitchId, config.SensorId));

            return true;
        }

        public bool DisableAutomation(Guid id)
        {
            var config = _configuration.TryGetById(id) as AutomatedSwitchApiConfig;
            if (config == null)
            {
                return false;
            }

            _eventPublisher.PublishEvent(new DisableBindingEvent(config.SwitchId, config.SensorId));

            return true;
        }
    }
}
