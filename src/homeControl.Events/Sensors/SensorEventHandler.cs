using System;
using homeControl.Core;
using homeControl.Events.Bindings;

namespace homeControl.Events.Sensors
{
    internal class SensorEventHandler : IHandler
    {
        private readonly IBindingController _bindingController;

        public SensorEventHandler(IBindingController bindingController)
        {
            _bindingController = bindingController;
        }
        
        public bool CanHandle(IEvent @event)
        {
            Guard.DebugAssertArgumentNotNull(@event, nameof(@event));
            return @event is AbstractSensorEvent;
        }

        public void Handle(IEvent @event)
        {
            Guard.DebugAssertArgumentNotNull(@event, nameof(@event));
            Guard.DebugAssertArgument(CanHandle(@event), nameof(@event));

            var sensorEvent = @event as AbstractSensorEvent;
            if (@event is SensorActivatedEvent)
            {
                _bindingController.ProcessSensorActivation(sensorEvent.SensorId);
            }
            else if (@event is SensorDeactivatedEvent)
            {
                _bindingController.ProcessSensorDeactivation(sensorEvent.SensorId);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
