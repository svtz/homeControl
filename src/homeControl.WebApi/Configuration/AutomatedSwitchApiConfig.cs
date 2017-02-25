using homeControl.Configuration.Sensors;

namespace homeControl.WebApi.Configuration
{
    public sealed class AutomatedSwitchApiConfig : SwitchApiConfig
    {
        public SensorId SensorId { get; set; }
    }
}