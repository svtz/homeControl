using homeControl.Configuration.Sensors;
using homeControl.WebApi.Configuration;

namespace homeControl.WebApi.Controllers
{
    public sealed class AutomatedSwitchApiConfig : SwitchApiConfig
    {
        public SensorId SensorId { get; set; }
    }
}