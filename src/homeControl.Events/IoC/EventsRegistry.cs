using homeControl.Core;
using homeControl.Events.Sensors;
using StructureMap;

namespace homeControl.Events.IoC
{
    public class EventsRegistry : Registry
    {
        public EventsRegistry()
        {
            ForSingletonOf<SensorWatcher>();
            For<IHandlerRepository>().Use<HandlerRepository>().Singleton();
            ForSingletonOf<EventPublisherAccessor>();
        }
    }
}
