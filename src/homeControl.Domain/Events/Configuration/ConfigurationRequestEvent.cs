using homeControl.Domain.Events;

namespace homeControl.Events.System
{
    public class ConfigurationRequestEvent : IEvent
    {
        public string ConfigurationKey { get; set; }
        public string ReplyAddress { get; set; }
    }
}