using System.Collections.Generic;
using homeControl.Configuration.Sensors;
using homeControl.Configuration.Switches;
using homeControl.Core;
using homeControl.Events.Bindings;
using Moq;
using Xunit;

namespace homeControl.Events.Tests
{
    public class BindingEventHandlerTests
    {
        public static IEnumerable<object[]> BindingEvents =>
            new[]
            {
                new object[] { new EnableBindingEvent(SwitchId.NewId(), SensorId.NewId()) },
                new object[] { new DisableBindingEvent(SwitchId.NewId(), SensorId.NewId())  }
            };

        [Theory]
        [MemberData(nameof(BindingEvents))]
        public void TestCanHandleSensorEvent(AbstractBindingEvent @event)
        {
            var handler = new BindingEventHandler(Mock.Of<IBindingStateManager>());

            Assert.True(handler.CanHandle(@event));
        }

        [Fact]
        public void TestCantHandleOtherEvents()
        {
            var handler = new BindingEventHandler(Mock.Of<IBindingStateManager>());
            var @event = Mock.Of<IEvent>();

            Assert.False(handler.CanHandle(@event));
        }

        [Fact]
        public void Test_WhenReceivedEnableEvent_ThenEnableBinding()
        {
            var @event = new EnableBindingEvent(SwitchId.NewId(), SensorId.NewId());
            var stateManagerMock = new Mock<IBindingStateManager>(MockBehavior.Strict);
            stateManagerMock.Setup(manager => manager.EnableBinding(@event.SwitchId, @event.SensorId));
            var handler = new BindingEventHandler(stateManagerMock.Object);

            handler.Handle(@event);

            stateManagerMock.Verify(manager => manager.EnableBinding(@event.SwitchId, @event.SensorId), Times.Once);
        }

        [Fact]
        public void Test_WhenReceivedDisableEvent_ThenDisableBinding()
        {
            var @event = new DisableBindingEvent(SwitchId.NewId(), SensorId.NewId());
            var stateManagerMock = new Mock<IBindingStateManager>(MockBehavior.Strict);
            stateManagerMock.Setup(manager => manager.DisableBinding(@event.SwitchId, @event.SensorId));
            var handler = new BindingEventHandler(stateManagerMock.Object);

            handler.Handle(@event);

            stateManagerMock.Verify(manager => manager.DisableBinding(@event.SwitchId, @event.SensorId), Times.Once);
        }
    }
}