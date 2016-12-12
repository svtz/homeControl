using homeControl.Core;
using StructureMap;

namespace homeControl.Events
{
    /// <summary> To avoid bi-directional dependency while resolving <see cref="Bus"/> </summary>
    internal sealed class EventPublisherAccessor
    {
        private readonly IContainer _container;

        public EventPublisherAccessor(IContainer container)
        {
            _container = container;
        }

        public IEventPublisher GetEventPublisher()
        {
            return _container.GetInstance<IEventPublisher>();
        }
    }
}