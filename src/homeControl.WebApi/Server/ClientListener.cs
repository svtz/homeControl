using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using homeControl.Configuration;
using homeControl.WebApi.Configuration;

namespace homeControl.WebApi.Server
{
    internal sealed class ClientListener : IClientListener
    {
        private readonly IClientListenerConfigurationRepository _configurationRepository;
        private readonly IClientsPool _clientsPool;
        private readonly IClientProcessorFactory _clientProcessorFactory;
        private readonly Lazy<TcpListener> _listener;
        private CancellationTokenSource _cts;

        public ClientListener(IClientListenerConfigurationRepository configurationRepository,
            IClientsPool clientsPool,
            IClientProcessorFactory clientProcessorFactory)
        {
            Guard.DebugAssertArgumentNotNull(configurationRepository, nameof(configurationRepository));
            _configurationRepository = configurationRepository;
            _clientsPool = clientsPool;
            _clientProcessorFactory = clientProcessorFactory;
            _listener = new Lazy<TcpListener>(CreateListener);
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

        private bool _running = false;
        public void StartListening()
        {
            CheckNotDisposed();
            if (_running)
            {
                return;
            }

            _running = true;
            _cts = new CancellationTokenSource();
            _listener.Value.Start();
            Task.Factory.StartNew(() => ListeningLoop(_cts.Token), _cts.Token);
        }

        /// <remarks>this does not disconnect clients already connected</remarks>
        public void StopListening()
        {
            CheckNotDisposed();
            if (!_running)
            {
                return;
            }

            _listener.Value.Stop();
            _cts.Cancel();

            _cts.Dispose();
            _cts = null;
            _running = false;
        }

        private void ListeningLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var connectionTask = _listener.Value.AcceptTcpClientAsync();
                connectionTask.Wait(ct);
                
                var processor = _clientProcessorFactory.Create(connectionTask.Result);
                processor.Disconnected += ProcessorDisconnected;
                _clientsPool.Add(processor);
                processor.Start();
            }

            ct.ThrowIfCancellationRequested();
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
            if (!_disposed)
            {
                StopListening();
                _disposed = true;
            }
        }

        private void CheckNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }
    }
}
