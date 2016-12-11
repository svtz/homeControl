using System;
using homeControl.Core;
using homeControl.Events.Sensors;
using homeControl.Events.Switches;

namespace homeControl.Events.Triggers
{
    internal class SensorTrigger : IHandler
    {
        private readonly IEventPublisher _eventPublisher;

        public SensorTrigger(IEventPublisher eventPublisher)
        {
            _eventPublisher = eventPublisher;
        }

        private Guid _switchId;
        public Guid SwitchId
        {
            get { return _switchId; }
            set
            {
                Guard.DebugAssertArgumentNotDefault(value, nameof(value));
                _switchId = value;
            }
        }

        private Guid _sensorId;
        public Guid SensorId
        {
            get { return _sensorId; }
            set
            {
                Guard.DebugAssertArgumentNotDefault(value, nameof(value));
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
