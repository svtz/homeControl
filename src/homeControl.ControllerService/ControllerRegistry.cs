using homeControl.ControllerService.Bindings;
using homeControl.ControllerService.Sensors;
using StructureMap;

namespace homeControl.ControllerService
{
    internal sealed class ControllerRegistry : Registry
    {
        public ControllerRegistry()
        {
            ForConcreteType<BindingController>().Configure.Transient();

            Forward<BindingController, IBindingController>();
            Forward<BindingController, IBindingStateManager>();

            ForConcreteType<BindingEventsProcessor>().Configure.Transient();
            ForConcreteType<SensorEventsProcessor>().Configure.Transient();
        }
    }
}