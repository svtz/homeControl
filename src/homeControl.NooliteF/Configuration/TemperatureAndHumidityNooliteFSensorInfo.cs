using homeControl.Domain;

namespace homeControl.NooliteF.Configuration
{
    internal sealed class TemperatureAndHumidityNooliteFSensorInfo : TemperatureNooliteFSensorInfo
    {
        public SensorId HumiditySensorId { get; set; }
    }
}