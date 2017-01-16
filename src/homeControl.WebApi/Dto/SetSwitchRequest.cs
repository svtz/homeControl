using System;

namespace homeControl.WebApi.Dto
{
    public sealed class SetSwitchRequest
    {
        public Guid Id { get; set; }

        public object Value { get; set; }
    }
}