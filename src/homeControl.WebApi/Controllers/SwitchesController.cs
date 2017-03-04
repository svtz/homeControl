using System;
using System.Linq;
using homeControl.Core;
using homeControl.Events.Bindings;
using homeControl.Events.Switches;
using homeControl.WebApi.Configuration;
using homeControl.WebApi.Dto;
using Microsoft.AspNetCore.Mvc;

namespace homeControl.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class SwitchesController : Controller
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

        [HttpGet]
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

        [HttpPut]
        public IActionResult SetValue(Guid id, object value)
        {
            var config = _configuration.TryGetById(id);
            if (config == null)
            {
                return BadRequest();
            }
            var strategy = _setValueStrategies.SingleOrDefault(s => s.CanHandle(config.Kind, value));
            if (strategy == null)
            {
                return BadRequest();
            }

            var e1 = strategy.CreateControlEvent(config.SwitchId, value);
            var e2 = strategy.CreateSetPowerEvent(config.SwitchId, value);

            _eventPublisher.PublishEvent(e1);
            _eventPublisher.PublishEvent(e2);

            return Ok();
        }

        [HttpPut]
        public IActionResult TurnOn(Guid id)
        {
            var config = _configuration.TryGetById(id);
            if (config == null)
            {
                return BadRequest();
            }

            _eventPublisher.PublishEvent(new TurnOnEvent(config.SwitchId));

            return Ok();
        }

        [HttpPut]
        public IActionResult TurnOff(Guid id)
        {
            var config = _configuration.TryGetById(id);
            if (config == null)
            {
                return BadRequest();
            }

            _eventPublisher.PublishEvent(new TurnOffEvent(config.SwitchId));

            return Ok();
        }

        [HttpPut]
        public IActionResult EnableAutomation(Guid id)
        {
            var config = _configuration.TryGetById(id) as AutomatedSwitchApiConfig;
            if (config == null)
            {
                return BadRequest();
            }

            _eventPublisher.PublishEvent(new EnableBindingEvent(config.SwitchId, config.SensorId));

            return Ok();
        }

        [HttpPut]
        public IActionResult DisableAutomation(Guid id)
        {
            var config = _configuration.TryGetById(id) as AutomatedSwitchApiConfig;
            if (config == null)
            {
                return BadRequest();
            }

            _eventPublisher.PublishEvent(new DisableBindingEvent(config.SwitchId, config.SensorId));

            return Ok();
        }
    }
}
