using System;
using System.Net.Sockets;
using System.Threading;

namespace homeControl.ClientApi.Server
{
    internal sealed class ClientProcessor : IClientProcessor
    {
        private readonly TcpClient _client;

        private bool _running = false;

        public ClientProcessor(TcpClient client)
        {
            Guard.DebugAssertArgumentNotNull(client, nameof(client));
            _client = client;
        }

        public void Start()
        {
            CheckNotDisposed();
            if (_running)
            {
                return;
            }

            _running = true;
        }

        public void Stop()
        {
            CheckNotDisposed();
            if (!_running)
                return;

            _running = false;
        }


        private bool _disposed = false;
        public void Dispose()
        {
            if (_disposed)
                return;

            Stop();
            _client.Dispose();
            _disposed = true;
        }

        private void CheckNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        public event EventHandler Disconnected;
        private void OnDisconnected()
        {
            var handler = Interlocked.CompareExchange(ref Disconnected, null, null);
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}