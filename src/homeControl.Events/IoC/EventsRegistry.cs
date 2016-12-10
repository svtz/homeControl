using System;
using homeControl.Core;
using homeControl.Events.Sensors;
using homeControl.Events.Switches;
using StructureMap;

namespace homeControl.Events.IoC
{
    public class EventsRegistry : Registry
    {
        public EventsRegistry()
        {
            ForSingletonOf<SensorWatcher>();
            For<IHandlerFactory>().Use<HandlerFactory>().Singleton();
        }
    }
}
