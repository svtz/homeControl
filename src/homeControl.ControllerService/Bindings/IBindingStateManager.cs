using homeControl.Domain;

namespace homeControl.ControllerService.Bindings
{
    internal interface IBindingStateManager
    {
        void EnableBinding(SwitchId switchId, SensorId sensorId);
        void DisableBinding(SwitchId switchId, SensorId sensorId);
    }
}