using System.Collections.Generic;
using homeControl.Configuration.Sensors;
using homeControl.Configuration.Switches;
using homeControl.Core;
using homeControl.Events.Sensors;
using homeControl.Events.Switches;
using Moq;
using Xunit;

namespace homeControl.Events.Tests
{
    public class SwitchToSensorBinderTests
    {
        public static IEnumerable<object[]> SensorActivationEvents =>
            new[]
            {
                new object[] { new SensorActivatedEvent(SensorId.NewId()) },
                new object[] { new SensorDeactivatedEvent(SensorId.NewId()) },
            };

        public static IEnumerable<object[]> SensorAutomationEvents =>
            new[]
            {
                new object[] { new EnableSensorAutomationEvent(SensorId.NewId()) },
                new object[] { new DisableSensorAutomationEvent(SensorId.NewId()) },
            };

        [Theory]
        [MemberData(nameof(SensorActivationEvents))]
        [MemberData(nameof(SensorAutomationEvents))]
        public void TestCanHandleSensorEvent(AbstractSensorEvent @event)
        {
            var handler = new SwitchToSensorBinderHandler(Mock.Of<IEventPublisher>())
            {
                SwitchId = SwitchId.NewId(),
                SensorId = @event.SensorId
            };

            Assert.True(handler.CanHandle(@event));
        }

        [Theory]
        [MemberData(nameof(SensorActivationEvents))]
        [MemberData(nameof(SensorAutomationEvents))]
        public void DontProcessEventWithNonMatchedId(AbstractSensorEvent @event)
        {
            var handler = new SwitchToSensorBinderHandler(Mock.Of<IEventPublisher>())
            {
                SwitchId = SwitchId.NewId(),
                SensorId = SensorId.NewId()
            };

            Assert.False(handler.CanHandle(@event));
        }

        [Fact]
        public void TestCantHandleOtherEvents()
        {
            var handler = new SwitchToSensorBinderHandler(Mock.Of<IEventPublisher>());
            var @event = Mock.Of<IEvent>();

            Assert.False(handler.CanHandle(@event));
        }

        [Fact]
        public void TestTurnOn()
        {
            var sensorId = SensorId.NewId();
            var sensorActivatedEvent = new SensorActivatedEvent(sensorId);
            var switchId = SwitchId.NewId();
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            publisherMock.Setup(publisher => publisher.PublishEvent(It.Is<TurnOnEvent>(e => e.SwitchId == switchId)));

            var handler = new SwitchToSensorBinderHandler(publisherMock.Object)
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
            var sensorId = SensorId.NewId();
            var sensorActivatedEvent = new SensorDeactivatedEvent(sensorId);
            var switchId = SwitchId.NewId();
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            publisherMock.Setup(publisher => publisher.PublishEvent(It.Is<TurnOffEvent>(e => e.SwitchId == switchId)));

            var handler = new SwitchToSensorBinderHandler(publisherMock.Object)
            {
                SwitchId = switchId,
                SensorId = sensorId
            };
            handler.Handle(sensorActivatedEvent);

            publisherMock.Verify(sc => sc.PublishEvent(It.IsAny<IEvent>()), Times.Once);
        }

        [Theory]
        [MemberData(nameof(SensorAutomationEvents))]
        public void Test_ChangingSensorRegime_ShouldNotCauseEventPublishing(AbstractSensorEvent @event)
        {
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            var handler = new SwitchToSensorBinderHandler(publisherMock.Object)
            {
                SwitchId = SwitchId.NewId(),
                SensorId = @event.SensorId
            };

            handler.Handle(@event);

            publisherMock.Verify(publisher => publisher.PublishEvent(It.IsAny<IEvent>()), Times.Never);
        }

        [Theory]
        [MemberData(nameof(SensorActivationEvents))]
        public void Test_WhenAutomationEnabled_ThenProcessEvents(AbstractSensorEvent @event)
        {
            var disableAutomationEvent = new DisableSensorAutomationEvent(@event.SensorId);
            var enableAutomationEvent = new EnableSensorAutomationEvent(@event.SensorId);
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            publisherMock.Setup(publisher => publisher.PublishEvent(It.IsAny<IEvent>()));
            var handler = new SwitchToSensorBinderHandler(publisherMock.Object)
            {
                SwitchId = SwitchId.NewId(),
                SensorId = @event.SensorId
            };
            handler.Handle(disableAutomationEvent);
            handler.Handle(enableAutomationEvent);

            handler.Handle(@event);

            publisherMock.Verify(publisher => publisher.PublishEvent(It.IsAny<IEvent>()), Times.Once);
        }

        [Theory]
        [MemberData(nameof(SensorActivationEvents))]
        public void Test_WhenAutomationDisabled_ThenDontProcessEvents(AbstractSensorEvent @event)
        {
            var disableAutomationEvent = new DisableSensorAutomationEvent(@event.SensorId);
            var enableAutomationEvent = new EnableSensorAutomationEvent(@event.SensorId);
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            publisherMock.Setup(publisher => publisher.PublishEvent(It.IsAny<IEvent>()));
            var handler = new SwitchToSensorBinderHandler(publisherMock.Object)
            {
                SwitchId = SwitchId.NewId(),
                SensorId = @event.SensorId
            };
            handler.Handle(enableAutomationEvent);
            handler.Handle(disableAutomationEvent);

            handler.Handle(@event);

            publisherMock.Verify(publisher => publisher.PublishEvent(It.IsAny<IEvent>()), Times.Never);
        }
    }
}