using homeControl.Domain;

namespace homeControl.ControllerService.Bindings
{
    internal interface IBindingController
    {
        void ProcessSensorActivation(SensorId sensorId);
        void ProcessSensorDeactivation(SensorId sensorId);
    }
}