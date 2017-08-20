using System.Threading.Tasks;
using homeControl.Domain;

namespace homeControl.ControllerService.Bindings
{
    internal interface IBindingStateManager
    {
        Task EnableBinding(SwitchId switchId, SensorId sensorId);
        Task DisableBinding(SwitchId switchId, SensorId sensorId);
    }
}