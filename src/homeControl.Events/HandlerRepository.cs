using System;
using System.Collections.Generic;
using System.Linq;
using homeControl.Core;
using homeControl.Events.Bindings;
using homeControl.Events.Sensors;
using homeControl.Events.Switches;
using homeControl.Peripherals;

namespace homeControl.Events
{
    internal sealed class HandlerRepository : IHandlerRepository
    {
        private readonly ISwitchControllerSelector _switchController;
        private readonly BindingControllerFactory _bindingControllerFactory;

        public IHandler[] GetHandlers()
        {
            return _handlerLazy.Value;
        }

        private readonly Lazy<IHandler[]> _handlerLazy;

        public HandlerRepository(
            ISwitchControllerSelector switchController,
            BindingControllerFactory bindingControllerFactory)
        {
            Guard.DebugAssertArgumentNotNull(switchController, nameof(switchController));
            Guard.DebugAssertArgumentNotNull(bindingControllerFactory, nameof(bindingControllerFactory));

            _switchController = switchController;
            _bindingControllerFactory = bindingControllerFactory;

            _handlerLazy = new Lazy<IHandler[]>(() => CreateHandlers().ToArray());
        }

        private IEnumerable<IHandler> CreateHandlers()
        {
            yield return new SwitchEventHandler(_switchController);

            var bindingController = _bindingControllerFactory.Create();
            yield return new SensorEventHandler(bindingController);
            yield return new BindingEventHandler(bindingController);
        }
    }
}
