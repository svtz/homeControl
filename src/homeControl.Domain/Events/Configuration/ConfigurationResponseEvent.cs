using System;

namespace homeControl.Domain.Events.Configuration
{
    public class ConfigurationResponseEvent : IEvent, IEventWithAddress
    {
        public string Configuration { get; set; }
        public string Address { get; set; }

        public override string ToString()
        {
            return $"{nameof(ConfigurationResponseEvent)} to:{Address}";
        }
    }
}