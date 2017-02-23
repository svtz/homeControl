using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using homeControl.Core;
using homeControl.WebApi.Configuration;
using homeControl.WebApi.Dto;
using Microsoft.AspNetCore.Mvc;

namespace homeControl.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class SwitchesController : Controller
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly Lazy<ReadOnlyDictionary<Guid, SwitchApiConfig>> _configuration;
        private ReadOnlyDictionary<Guid, SwitchApiConfig> Configruration => _configuration.Value;
        private readonly ISetSwitchValueStrategy[] _setValueStrategies;

        public SwitchesController(IEventPublisher eventPublisher, IClientApiConfigurationRepository configuration,
            ISetSwitchValueStrategy[] setSwitchValueStrategies)
        {
            Guard.DebugAssertArgumentNotNull(eventPublisher, nameof(eventPublisher));
            Guard.DebugAssertArgumentNotNull(configuration, nameof(configuration));

            _eventPublisher = eventPublisher;

            _configuration = new Lazy<ReadOnlyDictionary<Guid, SwitchApiConfig>>(() => LoadConfig(configuration));
            _setValueStrategies = setSwitchValueStrategies;
        }

        private ReadOnlyDictionary<Guid, SwitchApiConfig> LoadConfig(IClientApiConfigurationRepository configuration)
        {
            var configDict = configuration.GetClientApiConfig().ToDictionary(cfg => cfg.Id);
            return new ReadOnlyDictionary<Guid, SwitchApiConfig>(configDict);
        }


        // GET api/switches
        [HttpGet]
        public SwitchDto[] GetDescriptions()
        {
            return Configruration.Values.Select(CreateSwitchDto).ToArray();
        }

        private SwitchDto CreateSwitchDto(SwitchApiConfig config)
        {
            Guard.DebugAssertArgumentNotNull(config, nameof(config));

            return new SwitchDto
            {
                Automation = config is AutomatedSwitchApiConfig ? SwitchAutomation.Supported : SwitchAutomation.None,
                Kind = config.Kind,
                Description = config.Description,
                Id = config.Id,
                Name = config.Name
            };
        }

        // PUT api/switches
        [HttpPut]
        public IActionResult SetValue(Guid id, object value)
        {
            SwitchApiConfig config;
            if (!Configruration.TryGetValue(id, out config))
            {
                return BadRequest();
            }
            var strategy = _setValueStrategies.SingleOrDefault(s => s.CanHandle(config.Kind, value));
            if (strategy == null)
            {
                return BadRequest();
            }

            var e1 = strategy.CreateControlEvent(id, value);
            var e2 = strategy.CreateSetPowerEvent(id, value);

            _eventPublisher.PublishEvent(e1);
            _eventPublisher.PublishEvent(e2);

            return Ok();
        }

        // PUT api/switches
        [HttpPut]
        public HttpResponseMessage EnableAutomation(Guid id)
        {
            throw new NotImplementedException();
        }

        // PUT api/switches
        [HttpPut]
        public HttpResponseMessage DisableAutomation(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
