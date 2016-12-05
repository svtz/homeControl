using System;
using System.Collections.Generic;
using homeControl.Core.Events;
using homeControl.Core.Processors;
using Moq;
using Xunit;

namespace homeControl.Core.Tests
{
    public class SwitchEventHandlerTests
    {
        public static IEnumerable<object[]> AbstractSwitchEvents =>
            new[]
            {
                new object[] { new TurnOnEvent(Guid.NewGuid()), }, 
                new object[] { new TurnOffEvent(Guid.NewGuid()), }, 
            };

        [Theory]
        [MemberData(nameof(AbstractSwitchEvents))]
        public void TestCanHandleSwitchEvent(AbstractSwitchEvent @event)
        {
            var handler = new SwitchEventHandler(Mock.Of<ISwitchController>())
            {
                SwitchId = @event.SwitchId
            };

            Assert.True(handler.CanHandle(@event));
        }

        [Theory]
        [MemberData(nameof(AbstractSwitchEvents))]
        public void DontProcessEventWithNonMatchedId(AbstractSwitchEvent @event)
        {
            var handler = new SwitchEventHandler(Mock.Of<ISwitchController>())
            {
                SwitchId = Guid.NewGuid()
            };

            Assert.False(handler.CanHandle(@event));
            Assert.Throws<ArgumentException>(() => handler.Handle(@event));
        }

        [Fact]
        public void TestCantHandleOtherEvents()
        {
            var @event = Mock.Of<IEvent>();
            var handler = new SwitchEventHandler(Mock.Of<ISwitchController>());

            Assert.False(handler.CanHandle(@event));
            Assert.Throws<ArgumentException>(() => handler.Handle(@event));
        }

        [Fact]
        public void TestTurnOn()
        {
            var swicthId = Guid.NewGuid();
            var onEvent = new TurnOnEvent(swicthId);
            var switchControllerMock = new Mock<ISwitchController>();

            var handler = new SwitchEventHandler(switchControllerMock.Object)
            {
                SwitchId = swicthId
            };
            handler.Handle(onEvent);

            switchControllerMock.Verify(sc => sc.TurnOn(swicthId), Times.Once);
        }

        [Fact]
        public void TestTurnOff()
        {
            var swicthId = Guid.NewGuid();
            var onEvent = new TurnOffEvent(swicthId);
            var switchControllerMock = new Mock<ISwitchController>();

            var handler = new SwitchEventHandler(switchControllerMock.Object)
            {
                SwitchId = swicthId
            };
            handler.Handle(onEvent);

            switchControllerMock.Verify(sc => sc.TurnOff(swicthId), Times.Once);
        }
    }
}
