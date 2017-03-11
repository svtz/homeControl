using System.Net.Sockets;
using homeControl.ClientApi.Server;
using Moq;

namespace homeControl.ClientApi.Tests.Mocks
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