namespace homeControl.Domain.Events.Configuration
{
    public class ConfigurationRequestEvent : IEvent
    {
        public string ConfigurationKey { get; set; }
        public string ReplyAddress { get; set; }
    }
}