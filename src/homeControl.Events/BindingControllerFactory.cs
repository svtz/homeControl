using homeControl.Events.Bindings;
using StructureMap;

namespace homeControl.Events
{
    /// <summary> To avoid bi-directional dependency while resolving <see cref="Bus"/> </summary>
    internal sealed class BindingControllerFactory
    {
        private readonly IContainer _container;

        public BindingControllerFactory(IContainer container)
        {
            _container = container;
        }

        public BindingController Create()
        {
            return _container.GetInstance<BindingController>();
        }
    }
}