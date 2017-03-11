using System;
using homeControl.ClientServerShared.Dto;
using homeControl.Configuration.Switches;

namespace homeControl.ClientApi.Configuration
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