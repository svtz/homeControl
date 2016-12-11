using System;
using System.Collections.Generic;
using homeControl.Core;
using homeControl.Events.Sensors;
using homeControl.Events.Switches;
using homeControl.Events.Triggers;
using Moq;
using Xunit;

namespace homeControl.Events.Tests
{
    public class SensorTriggerTests
    {
        public static IEnumerable<object[]> AbstractSensorEvents =>
            new[]
            {
                new object[] { new SensorActivatedEvent(Guid.NewGuid()), },
                new object[] { new SensorDeactivatedEvent(Guid.NewGuid()), },
            };

        [Theory]
        [MemberData(nameof(AbstractSensorEvents))]
        public void TestCanHandleSensorEvent(AbstractSensorEvent @event)
        {
            var handler = new SensorTrigger(Mock.Of<IEventPublisher>())
            {
                SwitchId = Guid.NewGuid(),
                SensorId = @event.SensorId
            };

            Assert.True(handler.CanHandle(@event));
        }

        [Theory]
        [MemberData(nameof(AbstractSensorEvents))]
        public void DontProcessEventWithNonMatchedId(AbstractSensorEvent @event)
        {
            var handler = new SensorTrigger(Mock.Of<IEventPublisher>())
            {
                SwitchId = Guid.NewGuid(),
                SensorId = Guid.NewGuid()
            };

            Assert.False(handler.CanHandle(@event));
        }

        [Fact]
        public void TestCantHandleOtherEvents()
        {
            var handler = new SensorTrigger(Mock.Of<IEventPublisher>());
            var @event = Mock.Of<IEvent>();

            Assert.False(handler.CanHandle(@event));
        }

        [Fact]
        public void TestTurnOn()
        {
            var sensorId = Guid.NewGuid();
            var sensorActivatedEvent = new SensorActivatedEvent(sensorId);
            var switchId = Guid.NewGuid();
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            publisherMock.Setup(publisher => publisher.PublishEvent(It.Is<TurnOnEvent>(e => e.SwitchId == switchId)));

            var handler = new SensorTrigger(publisherMock.Object)
            {
                SwitchId = switchId,
                SensorId = sensorId
            };
            handler.Handle(sensorActivatedEvent);

            publisherMock.Verify(sc => sc.PublishEvent(It.IsAny<IEvent>()), Times.Once);
        }

        [Fact]
        public void TestTurnOff()
        {
            var sensorId = Guid.NewGuid();
            var sensorActivatedEvent = new SensorDeactivatedEvent(sensorId);
            var switchId = Guid.NewGuid();
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            publisherMock.Setup(publisher => publisher.PublishEvent(It.Is<TurnOffEvent>(e => e.SwitchId == switchId)));

            var handler = new SensorTrigger(publisherMock.Object)
            {
                SwitchId = switchId,
                SensorId = sensorId
            };
            handler.Handle(sensorActivatedEvent);

            publisherMock.Verify(sc => sc.PublishEvent(It.IsAny<IEvent>()), Times.Once);
        }
    }
}