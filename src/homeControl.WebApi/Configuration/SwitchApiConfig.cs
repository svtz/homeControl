using System;
using homeControl.Configuration.Switches;
using homeControl.WebApi.Dto;

namespace homeControl.WebApi.Configuration
{
    internal class SwitchApiConfig
    {
        public Guid ConfigId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public SwitchId SwitchId { get; set; }
        public SwitchKind Kind { get; set; }
    }
}