using homeControl.Configuration.Sensors;

namespace homeControl.Peripherals
{
    public interface ISensorGate
    {
        void OnSensorActivated(SensorId sensorId);
        void OnSensorDeactivated(SensorId sensorId);
    }
}