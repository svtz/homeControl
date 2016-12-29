using System;

namespace homeControl.WebApi.Dto
{
    public sealed class SwitchDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public SwitchKind Kind { get; set; }

        public SwitchAutomation Automation { get; set; }

        public SwitchValueType ValueType { get; set; }
    }
}