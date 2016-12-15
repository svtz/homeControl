using homeControl.Configuration.Sensors;
using homeControl.Core;
using homeControl.Events.Sensors;
using Moq;
using Xunit;

namespace homeControl.Events.Tests
{
    public class SensorGateTests
    {
        [Fact]
        public void Test_WhenSensorActivated_ThenPublishActivationEvent()
        {
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            var sensorId = SensorId.NewId();
            publisherMock.Setup(publisher => publisher.PublishEvent(It.Is<IEvent>(e => e is SensorActivatedEvent && ((SensorActivatedEvent)e).SensorId == sensorId)));
            var gate = new SensorGate(publisherMock.Object);

            gate.OnSensorActivated(sensorId);

            publisherMock.Verify(publisher => publisher.PublishEvent(It.IsAny<IEvent>()), Times.Once);
        }

        [Fact]
        public void Test_WhenSensorDeactivated_ThenPublishDeactivationEvent()
        {
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            var sensorId = SensorId.NewId();
            publisherMock.Setup(publisher => publisher.PublishEvent(It.Is<IEvent>(e => e is SensorDeactivatedEvent && ((SensorDeactivatedEvent)e).SensorId == sensorId)));
            var gate = new SensorGate(publisherMock.Object);

            gate.OnSensorDeactivated(sensorId);

            publisherMock.Verify(publisher => publisher.PublishEvent(It.IsAny<IEvent>()), Times.Once);
        }
    }
}
