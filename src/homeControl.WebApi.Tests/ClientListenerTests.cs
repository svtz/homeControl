using System;
using System.Net.Sockets;
using System.Threading;
using homeControl.WebApi.Server;
using homeControl.WebApi.Tests.Mocks;
using Moq;
using Xunit;

namespace homeControl.WebApi.Tests
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
            using (var listener = new ClientListener(new TestListenerConfigurationRepository(), clientsPoolMock.Object, factoryMock.Object))
            {
                listener.StartListening();

                using (var client = new TcpClient())
                {
                    client.ConnectAsync(
                            TestListenerConfigurationRepository.IPAddress,
                            TestListenerConfigurationRepository.PortNumber)
                        .Wait();

                    Assert.Equal(true, client.Connected);
                    Assert.True(clientConnectedEvent.WaitOne(_timeout));
                    clientsPoolMock.Verify(p => p.Add(processor), Times.Once());
                }

            }
        }

        [Fact]
        public void Test_CannotConnectWhenListenerNotStarted()
        {
            using (new ClientListener(new TestListenerConfigurationRepository(), Mock.Of<IClientsPool>(), Mock.Of<IClientProcessorFactory>()))
            {
                using (var client = new TcpClient())
                {
                    Assert.ThrowsAnyAsync<SocketException>(
                            () => client.ConnectAsync(
                                TestListenerConfigurationRepository.IPAddress,
                                TestListenerConfigurationRepository.PortNumber))
                        .Wait();
                }
            }
        }

        [Fact]
        public void Test_CannotConnectWhenListenerStopped()
        {
            using (var listener = new ClientListener(
                new TestListenerConfigurationRepository(),
                Mock.Of<IClientsPool>(),
                Mock.Of<IClientProcessorFactory>()))
            {
                listener.StartListening();
                listener.StopListening();

                using (var client = new TcpClient())
                {
                    Assert.ThrowsAnyAsync<SocketException>(
                            () => client.ConnectAsync(
                                TestListenerConfigurationRepository.IPAddress,
                                TestListenerConfigurationRepository.PortNumber))
                        .Wait();
                }
            }
        }

        [Fact]
        public void Test_ConnectAfterListenerRestarted()
        {
            var clientConnectedEvent = new ManualResetEvent(false);
            var processor = Mock.Of<IClientProcessor>();
            var clientsPoolMock = new ClientsPoolMock().CanAdd(processor, () => clientConnectedEvent.Set());
            var factoryMock = new ClientProcessorFactoryMock().CanCreate(processor);

            using (var listener = new ClientListener(new TestListenerConfigurationRepository(), clientsPoolMock.Object, factoryMock.Object))
            {
                listener.StartListening();
                listener.StopListening();
                listener.StartListening();

                using (var client = new TcpClient())
                {
                    client.ConnectAsync(
                            TestListenerConfigurationRepository.IPAddress,
                            TestListenerConfigurationRepository.PortNumber)
                        .Wait();

                    Assert.Equal(true, client.Connected);
                    Assert.True(clientConnectedEvent.WaitOne(_timeout));
                    clientsPoolMock.Verify(p => p.Add(processor), Times.Once());
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

            using (var listener = new ClientListener(new TestListenerConfigurationRepository(), clientsPoolMock.Object, factoryMock.Object))
            {
                listener.StartListening();

                using (var client = new TcpClient())
                {
                    client.ConnectAsync(
                            TestListenerConfigurationRepository.IPAddress,
                            TestListenerConfigurationRepository.PortNumber)
                        .Wait();
                }
                processorMock.Raise(p => p.Disconnected += null, processorMock.Object, null);

                Assert.True(clientDisconnectedEvent.WaitOne(_timeout));
                clientsPoolMock.Verify(p => p.Remove(processorMock.Object), Times.Once());
            }
        }
    }
}
