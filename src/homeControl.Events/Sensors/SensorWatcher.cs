using System;
using homeControl.Core;
using homeControl.Peripherals;

namespace homeControl.Events.Sensors
{
    internal sealed class SensorWatcher : IDisposable
    {
        private readonly ISensor _sensor;
        private readonly IEventPublisher _eventPublisher;

        public SensorWatcher(ISensor sensor, IEventPublisher eventPublisher)
        {
            Guard.DebugAssertArgumentNotNull(sensor, nameof(sensor));
            Guard.DebugAssertArgumentNotNull(eventPublisher, nameof(eventPublisher));

            _sensor = sensor;
            _eventPublisher = eventPublisher;

            _sensor.SensorActivated += OnSensorActivated;
            _sensor.SensorDeactivated += OnSensorDeactivated;
        }

        private void OnSensorActivated(object sender, SensorEventArgs sensorEventArgs)
        {
            Guard.DebugAssertArgumentNotNull(sensorEventArgs, nameof(sensorEventArgs));

            var id = new SensorId(sensorEventArgs.SensorId);
            _eventPublisher.PublishEvent(new SensorActivatedEvent(id));
        }

        private void OnSensorDeactivated(object sender, SensorEventArgs sensorEventArgs)
        {
            Guard.DebugAssertArgumentNotNull(sensorEventArgs, nameof(sensorEventArgs));

            var id = new SensorId(sensorEventArgs.SensorId);
            _eventPublisher.PublishEvent(new SensorDeactivatedEvent(id));
        }

        public void Dispose()
        {
            _sensor.SensorActivated -= OnSensorActivated;
            _sensor.SensorDeactivated -= OnSensorDeactivated;
        }
    }
}
