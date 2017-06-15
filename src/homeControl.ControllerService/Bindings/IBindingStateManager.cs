using homeControl.Configuration.Sensors;
using homeControl.Configuration.Switches;

namespace homeControl.Events.Bindings
{
    internal interface IBindingStateManager
    {
        void EnableBinding(SwitchId switchId, SensorId sensorId);
        void DisableBinding(SwitchId switchId, SensorId sensorId);
    }
}