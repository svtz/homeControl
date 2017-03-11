using homeControl.Configuration.Sensors;

namespace homeControl.ClientApi.Configuration
{
    internal sealed class AutomatedSwitchApiConfig : SwitchApiConfig
    {
        public SensorId SensorId { get; set; }
    }
}