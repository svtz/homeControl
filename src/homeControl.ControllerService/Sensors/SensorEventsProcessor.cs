using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using homeControl.Domain.Events;
using homeControl.Events.Bindings;
using JetBrains.Annotations;

namespace homeControl.Events.Sensors
{
    [UsedImplicitly]
    internal sealed class SensorEventsProcessor
    {
        private readonly IBindingController _bindingController;
        private readonly IEventSource _source;

        public SensorEventsProcessor(IBindingController bindingController, IEventSource source)
        {
            Guard.DebugAssertArgumentNotNull(bindingController, nameof(bindingController));
            Guard.DebugAssertArgumentNotNull(source, nameof(source));

            _bindingController = bindingController;
            _source = source;
        }

        public Task Run(CancellationToken ct)
        {
            return _source
                .GetMessages<AbstractSensorEvent>()
                .ForEachAsync(Handle, ct);
        }

        private void Handle(AbstractSensorEvent sensorEvent)
        {
            if (sensorEvent is SensorActivatedEvent)
            {
                _bindingController.ProcessSensorActivation(sensorEvent.SensorId);
            }
            else if (sensorEvent is SensorDeactivatedEvent)
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
