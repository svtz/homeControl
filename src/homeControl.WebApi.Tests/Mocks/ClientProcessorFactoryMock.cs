using System.Net.Sockets;
using homeControl.WebApi.Server;
using Moq;

namespace homeControl.WebApi.Tests.Mocks
{
    internal sealed class ClientProcessorFactoryMock : Mock<IClientProcessorFactory>
    {
        public ClientProcessorFactoryMock() : base(MockBehavior.Strict)
        {
        }

        public ClientProcessorFactoryMock CanCreate(IClientProcessor processor)
        {
            Setup(m => m.Create(It.IsAny<TcpClient>())).Returns(processor);
            return this;
        }
    }
}