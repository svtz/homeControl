using homeControl.Configuration.Sensors;

namespace homeControl.Events.Bindings
{
    internal interface IBindingController
    {
        void ProcessSensorActivation(SensorId sensorId);
        void ProcessSensorDeactivation(SensorId sensorId);
    }
}