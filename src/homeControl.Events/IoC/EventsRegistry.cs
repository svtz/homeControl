using homeControl.Core;
using homeControl.Events.Bindings.Configuration;
using homeControl.Events.Sensors;
using homeControl.Peripherals;
using Newtonsoft.Json;
using StructureMap;

namespace homeControl.Events.IoC
{
    public class EventsRegistry : Registry
    {
        public EventsRegistry()
        {
            For<ISensorGate>().Use<SensorGate>().Singleton();
            For<IHandlerRepository>().Use<HandlerRepository>().Singleton();

            For<JsonConverter>().Add<SwitchToSensorBindingSerializer>();
        }
    }
}
