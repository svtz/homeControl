using System;
using Moq;
using Xunit;
using homeControl.Core;

namespace homeControl.Core.Tests
{
    public class BusTests
    {
        [Fact]
        public void WhenCantHandle_ThenDontCallHandle()
        {
            var handlerMock = new Mock<IHandler>();
            handlerMock.Setup(m => m.CanHandle(It.IsAny<IMessage>())).Returns(false);

            var bus = new Bus(handlerMock.Object);
            bus.PostMessage(Mock.Of<IMessage>());
            bus.ProcessMessages();

            handlerMock.Verify(h => h.CanHandle(It.IsAny<IMessage>()), Times.Once);
            handlerMock.Verify(h => h.Handle(It.IsAny<IMessage>()), Times.Never);
        }

        [Fact]
        public void WhenCanHandle_ThanCallHandle()
        {
            var handlerMock = new Mock<IHandler>();
            handlerMock.Setup(m => m.CanHandle(It.IsAny<IMessage>())).Returns(true);

            var bus = new Bus(handlerMock.Object);
            bus.PostMessage(Mock.Of<IMessage>());
            bus.ProcessMessages();

            handlerMock.Verify(h => h.CanHandle(It.IsAny<IMessage>()), Times.Once);
            handlerMock.Verify(h => h.Handle(It.IsAny<IMessage>()), Times.Once);
        }

        [Fact]
        public void CallHandleOnlyForSuitableMessages()
        {
            var handlerMock = new Mock<IHandler>();
            var msgCanHandle = Mock.Of<IMessage>();
            handlerMock.Setup(m => m.CanHandle(msgCanHandle)).Returns(true);
            var msgCantHandle = Mock.Of<IMessage>();
            handlerMock.Setup(m => m.CanHandle(msgCantHandle)).Returns(false);

            var bus = new Bus(handlerMock.Object);
            bus.PostMessage(msgCanHandle);
            bus.PostMessage(msgCantHandle);
            bus.ProcessMessages();

            handlerMock.Verify(h => h.CanHandle(It.IsAny<IMessage>()), Times.Exactly(2));
            handlerMock.Verify(h => h.Handle(It.IsAny<IMessage>()), Times.Once);
        }

        [Fact]
        public void CallOnlySuitableHandlers()
        {
            var message = Mock.Of<IMessage>();
            var suitableHandlerMock = new Mock<IHandler>();
            suitableHandlerMock.Setup(m => m.CanHandle(message)).Returns(true);
            var unsuitableHandlerMock = new Mock<IHandler>();
            unsuitableHandlerMock.Setup(m => m.CanHandle(message)).Returns(false);

            var bus = new Bus(suitableHandlerMock.Object, unsuitableHandlerMock.Object);
            bus.PostMessage(message);
            bus.ProcessMessages();

            suitableHandlerMock.Verify(h => h.CanHandle(message), Times.Once);
            suitableHandlerMock.Verify(h => h.Handle(message), Times.Once);
            unsuitableHandlerMock.Verify(h => h.CanHandle(message), Times.Once);
            unsuitableHandlerMock.Verify(h => h.Handle(message), Times.Never);
        }
    }
}
