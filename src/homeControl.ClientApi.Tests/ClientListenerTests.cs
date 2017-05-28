using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using homeControl.ClientApi.Server;
using homeControl.ClientApi.Tests.Mocks;
using Moq;
using Xunit;

namespace homeControl.ClientApi.Tests
{
    public class ClientListenerTests
    {
        private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);

        [Fact]
        public void Test_ClientConnection()
        {
            var clientConnectedEvent = new ManualResetEvent(false);
            var processor = Mock.Of<IClientProcessor>();
            var clientsPoolMock = new ClientsPoolMock().CanAdd(processor, () => clientConnectedEvent.Set());
            var factoryMock = new ClientProcessorFactoryMock().CanCreate(processor);
            using (var cts = new CancellationTokenSource())
            using (new ClientListener(new TestListenerConfigurationRepository(), clientsPoolMock.Object, factoryMock.Object, cts.Token))
            {
                cts.CancelAfter(_timeout);

                using (var client = new TcpClient())
                {
                    client.ConnectAsync(
                            TestListenerConfigurationRepository.IPAddress,
                            TestListenerConfigurationRepository.PortNumber)
                        .Wait(cts.Token);

                    Assert.Equal(true, client.Connected);
                    Assert.True(clientConnectedEvent.WaitOne(_timeout));
                    clientsPoolMock.Verify(p => p.Add(processor), Times.Once());
                }

                cts.Cancel();
            }
        }

        [Fact]
        public void Test_CannotConnectWhenListenerDisposed()
        {
            using (var listener = new ClientListener(
                new TestListenerConfigurationRepository(), 
                Mock.Of<IClientsPool>(),
                Mock.Of<IClientProcessorFactory>(), 
                CancellationToken.None))
            {
                listener.Dispose();

                using (var client = new TcpClient())
                {
                    var result =
                        Assert.ThrowsAnyAsync<SocketException>(
                                () => client.ConnectAsync(
                                    TestListenerConfigurationRepository.IPAddress,
                                    TestListenerConfigurationRepository.PortNumber))
                            .Wait(_timeout);
                    Assert.True(result);
                }
            }
        }

        [Fact]
        public void Test_ClientDisconnection()
        {
            var clientDisconnectedEvent = new ManualResetEvent(false);
            var processorMock = new Mock<IClientProcessor>();
            var clientsPoolMock = new ClientsPoolMock().CanRemove(processorMock.Object, () => clientDisconnectedEvent.Set());
            var factoryMock = new ClientProcessorFactoryMock().CanCreate(processorMock.Object);

            using (new ClientListener(new TestListenerConfigurationRepository(), clientsPoolMock.Object, factoryMock.Object, CancellationToken.None))
            {

                using (var client = new TcpClient())
                {
                    var result = client.ConnectAsync(
                            TestListenerConfigurationRepository.IPAddress,
                            TestListenerConfigurationRepository.PortNumber)
                        .Wait(_timeout);

                    Assert.True(result);
                }
                processorMock.Raise(p => p.Disconnected += null, processorMock.Object, null);

                Assert.True(clientDisconnectedEvent.WaitOne(_timeout));
                clientsPoolMock.Verify(p => p.Remove(processorMock.Object), Times.Once());
            }
        }
    }
}
