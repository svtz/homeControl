using System;
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
        private readonly IClientApiConfigurationRepository _configuration;

        public SwitchesController(IEventPublisher eventPublisher, IClientApiConfigurationRepository configuration)
        {
            Guard.DebugAssertArgumentNotNull(eventPublisher, nameof(eventPublisher));
            Guard.DebugAssertArgumentNotNull(configuration, nameof(configuration));

            _eventPublisher = eventPublisher;
            _configuration = configuration;
        }

        // GET api/switches
        [HttpGet]
        public SwitchDto[] GetDescriptions()
        {
            return _configuration.GetClientApiConfig().Select(CreateSwitchDto).ToArray();
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
        public HttpResponseMessage Set(SetSwitchRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
