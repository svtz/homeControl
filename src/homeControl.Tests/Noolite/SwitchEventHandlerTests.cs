using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using homeControl.Domain;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Switches;
using homeControl.NooliteService;
using homeControl.NooliteService.SwitchController;
using Moq;
using Serilog;
using Xunit;

namespace homeControl.Tests.Noolite
{
    public class SwitchEventsProcessorTests
    {
        public static IEnumerable<object[]> AbstractSwitchEvents =>
            new[]
            {
                new object[] { new TurnOnEvent(SwitchId.NewId()), }, 
                new object[] { new TurnOffEvent(SwitchId.NewId()), }, 
                new object[] { new SetPowerEvent(SwitchId.NewId(), 0.333), }, 
            };

        [Fact]
        public void TestTurnOn()
        {
            var swicthId = SwitchId.NewId();
            var onEvent = new TurnOnEvent(swicthId);
            var switchControllerMock = new Mock<ISwitchController>(MockBehavior.Strict);
            switchControllerMock.Setup(cntr => cntr.CanHandleSwitch(swicthId)).Returns(true);
            switchControllerMock.Setup(cntr => cntr.TurnOn(swicthId));
            var eventsSourceMock = new Mock<IEventSource>(MockBehavior.Strict);
            eventsSourceMock.Setup(e => e.ReceiveEvents<AbstractSwitchEvent>()).Returns(Observable.Repeat(onEvent, 1));

            var handler = new SwitchEventsProcessor(switchControllerMock.Object, eventsSourceMock.Object, Mock.Of<ILogger>());
            handler.Run(CancellationToken.None);

            switchControllerMock.Verify(sc => sc.CanHandleSwitch(swicthId), Times.Once);
            switchControllerMock.Verify(sc => sc.TurnOn(swicthId), Times.Once);
        }

        [Fact]
        public void TestTurnOff()
        {
            var swicthId = SwitchId.NewId();
            var offEvent = new TurnOffEvent(swicthId);
            var switchControllerMock = new Mock<ISwitchController>(MockBehavior.Strict);
            switchControllerMock.Setup(cntr => cntr.CanHandleSwitch(swicthId)).Returns(true);
            switchControllerMock.Setup(cntr => cntr.TurnOff(swicthId));
            var eventsSourceMock = new Mock<IEventSource>(MockBehavior.Strict);
            eventsSourceMock.Setup(e => e.ReceiveEvents<AbstractSwitchEvent>()).Returns(Observable.Repeat(offEvent, 1));

            var handler = new SwitchEventsProcessor(switchControllerMock.Object, eventsSourceMock.Object, Mock.Of<ILogger>());
            handler.Run(CancellationToken.None);

            switchControllerMock.Verify(sc => sc.CanHandleSwitch(swicthId), Times.Once);
            switchControllerMock.Verify(sc => sc.TurnOff(swicthId), Times.Once);
        }

        [Fact]
        public void TestSetPower()
        {
            var swicthId = SwitchId.NewId();
            const double power = 0.4;
            var powerEvent = new SetPowerEvent(swicthId, power);
            var switchControllerMock = new Mock<ISwitchController>(MockBehavior.Strict);
            switchControllerMock.Setup(cntr => cntr.CanHandleSwitch(swicthId)).Returns(true);
            switchControllerMock.Setup(cntr => cntr.SetPower(swicthId, power));
            var eventsSourceMock = new Mock<IEventSource>(MockBehavior.Strict);
            eventsSourceMock.Setup(e => e.ReceiveEvents<AbstractSwitchEvent>()).Returns(Observable.Repeat(powerEvent, 1));

            var handler = new SwitchEventsProcessor(switchControllerMock.Object, eventsSourceMock.Object, Mock.Of<ILogger>());
            handler.Run(CancellationToken.None);

            switchControllerMock.Verify(sc => sc.CanHandleSwitch(swicthId), Times.Once);
            switchControllerMock.Verify(sc => sc.SetPower(swicthId, power), Times.Once);
        }

        [Theory]
        [MemberData(nameof(AbstractSwitchEvents))]
        public void Test_WhenUnknownSwitch_ThenDoNothing(AbstractSwitchEvent @event)
        {
            var switchControllerMock = new Mock<ISwitchController>(MockBehavior.Strict);
            switchControllerMock.Setup(cntr => cntr.CanHandleSwitch(It.IsAny<SwitchId>())).Returns(false);
            var eventsSourceMock = new Mock<IEventSource>(MockBehavior.Strict);
            eventsSourceMock.Setup(e => e.ReceiveEvents<AbstractSwitchEvent>()).Returns(Observable.Repeat(@event, 1));

            var handler = new SwitchEventsProcessor(switchControllerMock.Object, eventsSourceMock.Object, Mock.Of<ILogger>());
            handler.Run(CancellationToken.None);

            switchControllerMock.Verify(sc => sc.CanHandleSwitch(@event.SwitchId), Times.Once);
            switchControllerMock.Verify(sc => sc.SetPower(It.IsAny<SwitchId>(), It.IsAny<double>()), Times.Never);
            switchControllerMock.Verify(sc => sc.TurnOff(It.IsAny<SwitchId>()), Times.Never);
            switchControllerMock.Verify(sc => sc.TurnOn(It.IsAny<SwitchId>()), Times.Never);
        }
    }
}
