using homeControl.Domain.Events;

namespace homeControl.Events.System
{
    public class ConfigurationResponseEvent : IEvent, IEventWithAddress
    {
        public string Configuration { get; set; }
        public string Address { get; set; }
    }
}