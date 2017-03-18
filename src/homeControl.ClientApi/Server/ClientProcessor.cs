using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace homeControl.ClientApi.Server
{
    internal sealed class ClientProcessor : IClientProcessor, IClientWriter
    {
        private readonly TcpClient _client;
        private readonly MessageWriterPipeline _writerPipeline;

        private bool _running = false;
        

        public ClientProcessor(TcpClient client, IClientMessageSerializer messageSerializer)
        {
            Guard.DebugAssertArgumentNotNull(client, nameof(client));
            _client = client;

            _writerPipeline = new MessageWriterPipeline(this, messageSerializer);
        }

        public void Start()
        {
            CheckNotDisconnected();
            if (_running)
            {
                return;
            }

            _running = true;
        }

        public void Stop()
        {
            CheckNotDisconnected();
            if (!_running)
                return;

            _running = false;
        }


        #region Disconnection

        private bool _disconnected = false;
        private readonly object _disconnectionLock = new object();

        private void CheckNotDisconnected()
        {
            if (_disconnected)
                throw new InvalidOperationException("Client disconnected");
        }

        public event EventHandler Disconnected;
        private void OnDisconnected()
        {
            var handler = Interlocked.CompareExchange(ref Disconnected, null, null);
            handler?.Invoke(this, EventArgs.Empty);
        }

        private void HandleDisconnection()
        {
            if (_disconnected)
                return;

            lock (_disconnectionLock)
            {
                if (_disconnected)
                    return;

                try
                {
                    Stop();
                    OnDisconnected();
                }
                finally
                {
                    _writerPipeline.Dispose();
                    _client.Dispose();
                }
                
                _disconnected = true;
            }
        }

        #endregion


        #region IClientWriter

        async Task IClientWriter.WriteAsync(byte[] data)
        {
            if (_disconnected || !_running)
                return;

            try
            {
                await _client.GetStream().WriteAsync(data, 0, data.Length);
            }
            catch (SocketException)
            {
                HandleDisconnection();
            }
            catch (ObjectDisposedException)
            {
                Guard.DebugAssert(_disconnected, "Disconnection error");
            }
        }

        #endregion
    }
}