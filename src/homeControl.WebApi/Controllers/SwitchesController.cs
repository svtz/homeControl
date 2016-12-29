using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using homeControl.Configuration.Switches;
using homeControl.Core;
using homeControl.WebApi.Dto;
using Microsoft.AspNetCore.Mvc;

namespace homeControl.WebApi.Controllers
{
    public sealed class AbstractSwitchApiConfig
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public SwitchId SwitchId { get; set; }
    }

    public sealed class SwitchApiConfig : AbstractSwitchApiConfig
    {
        public SwitchId SwitchId { get; set; }
    }

    public interface IClientApiConfigurationRepository
    {
        SwitchApiConfig[] GetClientApiConfig();
    }

    [Route("api/[controller]")]
    public class SwitchesController : Controller
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly IClientApiConfigurationRepository _configuration;

        public SwitchesController(IEventPublisher eventPublisher, IClientApiConfigurationRepository configuration)
        {
            _eventPublisher = eventPublisher;
            _configuration = configuration;
        }

        // GET api/switches
        [HttpGet]
        public SwitchDto[] GetAll()
        {
            throw new NotImplementedException();
        }

        // PUT api/switches
        [HttpPut]
        public HttpResponseMessage Set(SetSwitchRequest request)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class SetSwitchRequest
    {
        public Guid SwitchId { get; set; }

        public object Value { get; set; }
    }
}
