using System;
using homeControl.Configuration.Sensors;
using homeControl.Core;
using homeControl.Events.Sensors;
using homeControl.Peripherals;
using Moq;
using Xunit;

namespace homeControl.Events.Tests
{
    public class SensorWatcherTests
    {
        [Fact]
        public void Test_WhenSensorActivated_ThenPublishActivationEvent()
        {
            var sensorMock = new Mock<ISensor>();
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            var sensorId = SensorId.NewId();
            publisherMock.Setup(publisher => publisher.PublishEvent(It.Is<IEvent>(e => e is SensorActivatedEvent && ((SensorActivatedEvent)e).SensorId == sensorId)));
            var watcher = new SensorWatcher(sensorMock.Object, publisherMock.Object);

            sensorMock.Raise(sensor => sensor.SensorActivated += null, new SensorEventArgs(sensorId));

            publisherMock.Verify(publisher => publisher.PublishEvent(It.IsAny<IEvent>()), Times.Once);
        }

        [Fact]
        public void Test_WhenSensorDeactivated_ThenPublishDeactivationEvent()
        {
            var sensorMock = new Mock<ISensor>();
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            var sensorId = SensorId.NewId();
            publisherMock.Setup(publisher => publisher.PublishEvent(It.Is<IEvent>(e => e is SensorDeactivatedEvent && ((SensorDeactivatedEvent)e).SensorId == sensorId)));
            var watcher = new SensorWatcher(sensorMock.Object, publisherMock.Object);

            sensorMock.Raise(sensor => sensor.SensorDeactivated += null, new SensorEventArgs(sensorId));

            publisherMock.Verify(publisher => publisher.PublishEvent(It.IsAny<IEvent>()), Times.Once);
        }
    }
}
