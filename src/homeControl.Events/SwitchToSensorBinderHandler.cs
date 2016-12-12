using System;
using homeControl.Configuration.Sensors;
using homeControl.Configuration.Switches;
using homeControl.Core;
using homeControl.Events.Sensors;
using homeControl.Events.Switches;

namespace homeControl.Events
{
    internal class SwitchToSensorBinderHandler : IHandler
    {
        private readonly IEventPublisher _eventPublisher;

        public SwitchToSensorBinderHandler(IEventPublisher eventPublisher)
        {
            _eventPublisher = eventPublisher;
        }

        private SwitchId _switchId;
        public SwitchId SwitchId
        {
            get { return _switchId; }
            set
            {
                Guard.DebugAssertArgumentNotNull(value, nameof(value));
                _switchId = value;
            }
        }

        private SensorId _sensorId;
        public SensorId SensorId
        {
            get { return _sensorId; }
            set
            {
                Guard.DebugAssertArgumentNotNull(value, nameof(value));
                _sensorId = value;
            }
        }

        public bool CanHandle(IEvent @event)
        {
            Guard.DebugAssertArgumentNotNull(@event, nameof(@event));

            var sensorEvent = @event as AbstractSensorEvent;
            return sensorEvent != null && sensorEvent.SensorId == SensorId;
        }

        public void Handle(IEvent @event)
        {
            Guard.DebugAssertArgumentNotNull(@event, nameof(@event));
            Guard.DebugAssertArgument(CanHandle(@event), nameof(@event));

            if (@event is SensorActivatedEvent)
            {
                _eventPublisher.PublishEvent(new TurnOnEvent(SwitchId));
            }
            else if (@event is SensorDeactivatedEvent)
            {
                _eventPublisher.PublishEvent(new TurnOffEvent(SwitchId));
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
