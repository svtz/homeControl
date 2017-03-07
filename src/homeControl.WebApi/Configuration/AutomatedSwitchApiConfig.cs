using homeControl.Configuration.Sensors;

namespace homeControl.WebApi.Configuration
{
    internal sealed class AutomatedSwitchApiConfig : SwitchApiConfig
    {
        public SensorId SensorId { get; set; }
    }
}