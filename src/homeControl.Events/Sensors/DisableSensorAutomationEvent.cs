using homeControl.Configuration.Sensors;

namespace homeControl.Events.Sensors
{
    public class DisableSensorAutomationEvent : AbstractSensorEvent
    {
        public DisableSensorAutomationEvent(SensorId sensorId) : base(sensorId)
        {
        }
    }
}