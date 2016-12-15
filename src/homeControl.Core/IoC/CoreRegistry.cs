using StructureMap;

namespace homeControl.Core.IoC
{
    public sealed class CoreRegistry : Registry
    {
        public CoreRegistry()
        {
            ForSingletonOf<Bus>();
            Forward<Bus, IEventProcessor>();
            Forward<Bus, IEventPublisher>();
        }
    }
}
