using Moq;
using Xunit;

namespace homeControl.Core.Tests
{
    public class BusTests
    {
        [Fact]
        public void WhenCantHandle_ThenDontCallHandle()
        {
            var handlerMock = new Mock<IHandler>();
            handlerMock.Setup(h => h.CanHandle(It.IsAny<IEvent>())).Returns(false);
            var handlerFactoryMock = new Mock<IHandlerRepository>();
            handlerFactoryMock.Setup(factory => factory.GetHandlers()).Returns(new[] {handlerMock.Object});

            var bus = new Bus(handlerFactoryMock.Object);
            bus.PublishEvent(Mock.Of<IEvent>());
            bus.ProcessEvents();

            handlerMock.Verify(h => h.CanHandle(It.IsAny<IEvent>()), Times.Once);
            handlerMock.Verify(h => h.Handle(It.IsAny<IEvent>()), Times.Never);
        }

        [Fact]
        public void WhenCanHandle_ThanCallHandle()
        {
            var handlerMock = new Mock<IHandler>();
            handlerMock.Setup(h => h.CanHandle(It.IsAny<IEvent>())).Returns(true);
            var handlerFactoryMock = new Mock<IHandlerRepository>();
            handlerFactoryMock.Setup(factory => factory.GetHandlers()).Returns(new[] { handlerMock.Object });

            var bus = new Bus(handlerFactoryMock.Object);
            bus.PublishEvent(Mock.Of<IEvent>());
            bus.ProcessEvents();

            handlerMock.Verify(h => h.CanHandle(It.IsAny<IEvent>()), Times.Once);
            handlerMock.Verify(h => h.Handle(It.IsAny<IEvent>()), Times.Once);
        }

        [Fact]
        public void CallHandleOnlyForSuitableMessages()
        {
            var handlerMock = new Mock<IHandler>();
            var eventCanHandle = Mock.Of<IEvent>();
            handlerMock.Setup(h => h.CanHandle(eventCanHandle)).Returns(true);
            var msgCantHandle = Mock.Of<IEvent>();
            handlerMock.Setup(h => h.CanHandle(msgCantHandle)).Returns(false);
            var handlerFactoryMock = new Mock<IHandlerRepository>();
            handlerFactoryMock.Setup(factory => factory.GetHandlers()).Returns(new[] { handlerMock.Object });

            var bus = new Bus(handlerFactoryMock.Object);
            bus.PublishEvent(eventCanHandle);
            bus.PublishEvent(msgCantHandle);
            bus.ProcessEvents();

            handlerMock.Verify(h => h.CanHandle(It.IsAny<IEvent>()), Times.Exactly(2));
            handlerMock.Verify(h => h.Handle(It.IsAny<IEvent>()), Times.Once);
        }

        [Fact]
        public void CallOnlySuitableHandlers()
        {
            var @event = Mock.Of<IEvent>();
            var suitableHandlerMock = new Mock<IHandler>();
            suitableHandlerMock.Setup(h => h.CanHandle(@event)).Returns(true);
            var unsuitableHandlerMock = new Mock<IHandler>();
            unsuitableHandlerMock.Setup(h => h.CanHandle(@event)).Returns(false);
            var handlerFactoryMock = new Mock<IHandlerRepository>();
            handlerFactoryMock.Setup(factory => factory.GetHandlers()).Returns(new[] { suitableHandlerMock.Object, unsuitableHandlerMock.Object });

            var bus = new Bus(handlerFactoryMock.Object);
            bus.PublishEvent(@event);
            bus.ProcessEvents();

            suitableHandlerMock.Verify(h => h.CanHandle(@event), Times.Once);
            suitableHandlerMock.Verify(h => h.Handle(@event), Times.Once);
            unsuitableHandlerMock.Verify(h => h.CanHandle(@event), Times.Once);
            unsuitableHandlerMock.Verify(h => h.Handle(@event), Times.Never);
        }
    }
}
