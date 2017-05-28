using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using homeControl.ClientApi.Configuration;
using homeControl.Configuration;

namespace homeControl.ClientApi.Server
{
    internal sealed class ClientListener : IInitializer, IDisposable
    {
        private readonly IClientListenerConfigurationRepository _configurationRepository;
        private readonly IClientsPool _clientsPool;
        private readonly IClientProcessorFactory _clientProcessorFactory;
        private readonly CancellationToken _ct;
        private readonly TcpListener _listener;

        public ClientListener(IClientListenerConfigurationRepository configurationRepository,
            IClientsPool clientsPool,
            IClientProcessorFactory clientProcessorFactory,
            CancellationToken ct)
        {
            Guard.DebugAssertArgumentNotNull(configurationRepository, nameof(configurationRepository));
            ct.ThrowIfCancellationRequested();
            
            _configurationRepository = configurationRepository;
            _clientsPool = clientsPool;
            _clientProcessorFactory = clientProcessorFactory;
            _ct = ct;
            _listener = CreateListener();
            _listener.Start();
            Task.Factory.StartNew(ListeningLoop, _ct);
        }

        private TcpListener CreateListener()
        {
            var configuration = _configurationRepository.Get();
            IPAddress address;
            if (!IPAddress.TryParse(configuration.IPAddress, out address))
            {
                throw new InvalidConfigurationException("Could not parse IP address in the configuration file.");
            }

            return new TcpListener(address, configuration.Port);
        }

        private void ListeningLoop()
        {
            while (!_disposed)
            {
                var connectionTask = _listener.AcceptTcpClientAsync();
                connectionTask.Wait(_ct);
                
                var processor = _clientProcessorFactory.Create(connectionTask.Result);
                processor.Disconnected += ProcessorDisconnected;
                _clientsPool.Add(processor);
                processor.Start();
            }
        }

        private void ProcessorDisconnected(object sender, EventArgs eventArgs)
        {
            Guard.DebugAssertArgumentNotNull(sender, nameof(sender));
            Guard.DebugAssertArgument(sender is IClientProcessor, nameof(sender));

            var client = (IClientProcessor)sender;
            client.Disconnected -= ProcessorDisconnected;

            _clientsPool.Remove(client);
        }

        private bool _disposed = false;
        public void Dispose()
        {
            if (_disposed) return;

            _listener.Stop();
            _disposed = true;
        }

        public void Init()
        {
        }
    }
}
