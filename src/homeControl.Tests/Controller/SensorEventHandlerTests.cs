using System.Reactive.Linq;
using System.Threading;
using homeControl.Configuration.Sensors;
using homeControl.Domain.Events;
using homeControl.Events.Bindings;
using homeControl.Events.Sensors;
using Moq;
using Xunit;

namespace homeControl.Tests.Controller
{
    public class SensorEventsProcessorTests
    {
        [Fact]
        public void TestActivation()
        {
            var sensorId = SensorId.NewId();
            var sensorActivatedEvent = new SensorActivatedEvent(sensorId);
            var controllerMock = new Mock<IBindingController>(MockBehavior.Strict);
            controllerMock.Setup(controller => controller.ProcessSensorActivation(sensorId));
            var eventsSourceMock = new Mock<IEventSource>(MockBehavior.Strict);
            eventsSourceMock.Setup(e => e.ReceiveEvents<AbstractSensorEvent>()).Returns(Observable.Repeat(sensorActivatedEvent, 1));

            var handler = new SensorEventsProcessor(controllerMock.Object, eventsSourceMock.Object);
            handler.Run(CancellationToken.None);

            controllerMock.Verify(controller => controller.ProcessSensorActivation(sensorId), Times.Once);
        }

        [Fact]
        public void TestDeactivation()
        {
            var sensorId = SensorId.NewId();
            var sensorDeactivatedEvent = new SensorDeactivatedEvent(sensorId);
            var controllerMock = new Mock<IBindingController>(MockBehavior.Strict);
            controllerMock.Setup(controller => controller.ProcessSensorDeactivation(sensorId));
            var eventsSourceMock = new Mock<IEventSource>(MockBehavior.Strict);
            eventsSourceMock.Setup(e => e.ReceiveEvents<AbstractSensorEvent>()).Returns(Observable.Repeat(sensorDeactivatedEvent, 1));

            var handler = new SensorEventsProcessor(controllerMock.Object, eventsSourceMock.Object);
            handler.Run(CancellationToken.None);

            controllerMock.Verify(controller => controller.ProcessSensorDeactivation(sensorId), Times.Once);
        }
    }
}