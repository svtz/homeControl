using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace homeControl.ClientApi.Server
{
    internal sealed class ClientProcessor : IClientProcessor, IClientWriter
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

        
        #region IClientWriter

        private bool _disconnected = false;
        private readonly object _disconnectionLock = new object();
        private void HandleDisconnection()
        {
            if (_disconnected)
                return;

            lock (_disconnectionLock)
            {
                if (_disconnected)
                    return;

                Stop();
                OnDisconnected();
                _disconnected = true;
            }
        }

        public async Task WriteAsync(byte[] data)
        {
            if (_disposed || !_running)
                return;

            try
            {
                await _client.GetStream().WriteAsync(data, 0, data.Length);
            }
            catch (SocketException)
            {
                HandleDisconnection();
            }
        }

        #endregion
    }
}