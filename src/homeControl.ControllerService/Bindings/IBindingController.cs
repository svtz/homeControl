using System.Threading.Tasks;
using homeControl.Domain;

namespace homeControl.ControllerService.Bindings
{
    internal interface IBindingController
    {
        Task ProcessSensorActivation(SensorId sensorId);
        Task ProcessSensorDeactivation(SensorId sensorId);
        Task ProcessSensorValue(SensorId sensorId, decimal value);
    }
}