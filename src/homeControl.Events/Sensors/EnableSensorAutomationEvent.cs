using homeControl.Configuration.Sensors;

namespace homeControl.Events.Sensors
{
    public class EnableSensorAutomationEvent : AbstractSensorEvent
    {
        public EnableSensorAutomationEvent(SensorId sensorId) : base(sensorId)
        {
        }
    }
}