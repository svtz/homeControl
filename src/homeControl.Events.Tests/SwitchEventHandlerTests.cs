using System;
using System.Collections.Generic;
using homeControl.Core;
using homeControl.Events.Switches;
using homeControl.Peripherals;
using Moq;
using Xunit;

namespace homeControl.Events.Tests
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
            var switchControllerMock = new Mock<ISwitchController>(MockBehavior.Strict);
            switchControllerMock.Setup(cntr => cntr.CanHandleSwitch(@event.SwitchId)).Returns(true);
            var handler = new SwitchEventHandler(switchControllerMock.Object)
            {
                SwitchId = @event.SwitchId
            };

            Assert.True(handler.CanHandle(@event));
            switchControllerMock.Verify(cntr => cntr.CanHandleSwitch(It.IsAny<Guid>()), Times.Once);
        }

        [Theory]
        [MemberData(nameof(AbstractSwitchEvents))]
        public void DontProcessEventWithNonMatchedId(AbstractSwitchEvent @event)
        {
            var switchControllerMock = new Mock<ISwitchController>(MockBehavior.Strict);
            var handler = new SwitchEventHandler(switchControllerMock.Object)
            {
                SwitchId = Guid.NewGuid()
            };

            Assert.False(handler.CanHandle(@event));
        }

        [Fact]
        public void TestCantHandleOtherEvents()
        {
            var switchControllerMock = new Mock<ISwitchController>(MockBehavior.Strict);
            var handler = new SwitchEventHandler(switchControllerMock.Object);
            var @event = Mock.Of<IEvent>();

            Assert.False(handler.CanHandle(@event));
        }

        [Theory]
        [MemberData(nameof(AbstractSwitchEvents))]
        public void TestHandlerCantHandleEventIfControllerCant(AbstractSwitchEvent @event)
        {
            var switchControllerMock = new Mock<ISwitchController>(MockBehavior.Strict);
            switchControllerMock.Setup(cntr => cntr.CanHandleSwitch(@event.SwitchId)).Returns(false);
            var handler = new SwitchEventHandler(switchControllerMock.Object)
            {
                SwitchId = @event.SwitchId
            };

            Assert.False(handler.CanHandle(@event));
            switchControllerMock.Verify(cntr => cntr.CanHandleSwitch(It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public void TestTurnOn()
        {
            var swicthId = Guid.NewGuid();
            var onEvent = new TurnOnEvent(swicthId);
            var switchControllerMock = new Mock<ISwitchController>(MockBehavior.Strict);
            switchControllerMock.Setup(cntr => cntr.CanHandleSwitch(swicthId)).Returns(true);
            switchControllerMock.Setup(cntr => cntr.TurnOn(swicthId));

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
            var switchControllerMock = new Mock<ISwitchController>(MockBehavior.Strict);
            switchControllerMock.Setup(cntr => cntr.CanHandleSwitch(swicthId)).Returns(true);
            switchControllerMock.Setup(cntr => cntr.TurnOff(swicthId));

            var handler = new SwitchEventHandler(switchControllerMock.Object)
            {
                SwitchId = swicthId
            };
            handler.Handle(onEvent);

            switchControllerMock.Verify(sc => sc.TurnOff(swicthId), Times.Once);
        }
    }
}
