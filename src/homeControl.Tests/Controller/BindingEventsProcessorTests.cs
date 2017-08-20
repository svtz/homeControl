using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using homeControl.ControllerService.Bindings;
using homeControl.Domain;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Bindings;
using Moq;
using Serilog;
using Xunit;

namespace homeControl.Tests.Controller
{
    public class BindingEventsProcessorTests
    {
        [Fact]
        public void Test_WhenReceivedEnableEvent_ThenEnableBinding()
        {
            var @event = new EnableBindingEvent(SwitchId.NewId(), SensorId.NewId());
            var stateManagerMock = new Mock<IBindingStateManager>(MockBehavior.Strict);
            stateManagerMock.Setup(manager => manager.EnableBinding(@event.SwitchId, @event.SensorId)).Returns(Task.CompletedTask);
            var eventsSourceMock = new Mock<IEventSource>(MockBehavior.Strict);
            eventsSourceMock.Setup(e => e.ReceiveEvents<AbstractBindingEvent>()).Returns(Observable.Repeat(@event, 1));
            var handler = new BindingEventsProcessor(stateManagerMock.Object, eventsSourceMock.Object, Mock.Of<ILogger>());

            handler.Run(CancellationToken.None);

            stateManagerMock.Verify(manager => manager.EnableBinding(@event.SwitchId, @event.SensorId), Times.Once);
        }

        [Fact]
        public void Test_WhenReceivedDisableEvent_ThenDisableBinding()
        {
            var @event = new DisableBindingEvent(SwitchId.NewId(), SensorId.NewId());
            var stateManagerMock = new Mock<IBindingStateManager>(MockBehavior.Strict);
            stateManagerMock.Setup(manager => manager.DisableBinding(@event.SwitchId, @event.SensorId)).Returns(Task.CompletedTask); ;
            var eventsSourceMock = new Mock<IEventSource>(MockBehavior.Strict);
            eventsSourceMock.Setup(e => e.ReceiveEvents<AbstractBindingEvent>()).Returns(Observable.Repeat(@event, 1));
            var handler = new BindingEventsProcessor(stateManagerMock.Object, eventsSourceMock.Object, Mock.Of<ILogger>());

            handler.Run(CancellationToken.None);

            stateManagerMock.Verify(manager => manager.DisableBinding(@event.SwitchId, @event.SensorId), Times.Once);
        }
    }
}