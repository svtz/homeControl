using System.Collections.Generic;
using homeControl.Configuration.Sensors;
using homeControl.Core;
using homeControl.Events.Bindings;
using homeControl.Events.Sensors;
using Moq;
using Xunit;

namespace homeControl.Events.Tests
{
    public class SensorEventHandlerTests
    {
        public static IEnumerable<object[]> SensorActivationEvents =>
            new[]
            {
                new object[] { new SensorActivatedEvent(SensorId.NewId()) },
                new object[] { new SensorDeactivatedEvent(SensorId.NewId()) },
            };

        [Theory]
        [MemberData(nameof(SensorActivationEvents))]
        public void TestCanHandleSensorEvent(AbstractSensorEvent @event)
        {
            var handler = new SensorEventHandler(Mock.Of<IBindingController>());

            Assert.True(handler.CanHandle(@event));
        }

        [Fact]
        public void TestCantHandleOtherEvents()
        {
            var handler = new SensorEventHandler(Mock.Of<IBindingController>());
            var @event = Mock.Of<IEvent>();

            Assert.False(handler.CanHandle(@event));
        }

        [Fact]
        public void TestActivation()
        {
            var sensorId = SensorId.NewId();
            var sensorActivatedEvent = new SensorActivatedEvent(sensorId);
            var controllerMock = new Mock<IBindingController>(MockBehavior.Strict);
            controllerMock.Setup(controller => controller.ProcessSensorActivation(sensorId));

            var handler = new SensorEventHandler(controllerMock.Object);
            handler.Handle(sensorActivatedEvent);

            controllerMock.Verify(controller => controller.ProcessSensorActivation(sensorId), Times.Once);
        }

        [Fact]
        public void TestDeactivation()
        {
            var sensorId = SensorId.NewId();
            var sensorDeactivatedEvent = new SensorDeactivatedEvent(sensorId);
            var controllerMock = new Mock<IBindingController>(MockBehavior.Strict);
            controllerMock.Setup(controller => controller.ProcessSensorDeactivation(sensorId));

            var handler = new SensorEventHandler(controllerMock.Object);
            handler.Handle(sensorDeactivatedEvent);

            controllerMock.Verify(controller => controller.ProcessSensorDeactivation(sensorId), Times.Once);
        }
    }
}