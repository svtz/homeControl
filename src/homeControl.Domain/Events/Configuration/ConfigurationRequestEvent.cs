namespace homeControl.Domain.Events.Configuration
{
    public class ConfigurationRequestEvent : IEvent
    {
        public string ConfigurationKey { get; set; }
        public string ReplyAddress { get; set; }

        public override string ToString()
        {
            return $"{nameof(ConfigurationRequestEvent)} {ConfigurationKey} replyTo:{ReplyAddress}";
        }
    }
}