using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace homeControl.ClientApi.Server
{
    internal sealed class ClientProcessor : IClientProcessor, IClientWriter, IClientReader
    {
        private readonly TcpClient _client;
        private readonly IClientMessageSerializer _messageSerializer;
        private readonly IClientRequestRouter _requestRouter;
        private MessageWriterPipeline _writerPipeline;
        private MessageReaderPipeline _readerPipeline;

        private readonly CancellationTokenSource _cts;
        private readonly object _stateLock = new object();
            

        public ClientProcessor(TcpClient client, IClientMessageSerializer messageSerializer, IClientRequestRouter requestRouter, 
            CancellationToken ct)
        {
            Guard.DebugAssertArgumentNotNull(client, nameof(client));
            Guard.DebugAssertArgumentNotNull(messageSerializer, nameof(messageSerializer));
            Guard.DebugAssertArgumentNotNull(requestRouter, nameof(requestRouter));

            ct.ThrowIfCancellationRequested();

            _client = client;
            _messageSerializer = messageSerializer;
            _requestRouter = requestRouter;

            _cts = new CancellationTokenSource();
            ct.Register(HandleDisconnection);
        }

        private bool _canStart = true;
        public void Start()
        {
            if (!_canStart) return;
            lock (_stateLock)
            {
                if (!_canStart) return;
                _canStart = false;

                _writerPipeline = new MessageWriterPipeline(this, _messageSerializer, _cts.Token);
                _readerPipeline = new MessageReaderPipeline(this, _messageSerializer, _requestRouter, _cts.Token);
            }
        }


        #region Disconnection

        public event EventHandler Disconnected;
        private void OnDisconnected()
        {
            var handler = Interlocked.CompareExchange(ref Disconnected, null, null);
            handler?.Invoke(this, EventArgs.Empty);
        }

        private bool _disconnected = false;
        private void HandleDisconnection()
        {
            if (_disconnected)
                return;

            lock (_stateLock)
            {
                if (_disconnected)
                    return;

                _disconnected = true;
                _canStart = false;

                try
                {
                    OnDisconnected();
                }
                finally
                {
                    _writerPipeline?.Dispose();
                    _readerPipeline?.Dispose();

                    _cts.Cancel();
                    _cts.Dispose();
                    _client.Dispose();
                }
            }
        }

        #endregion


        #region IClientWriter

        async Task IClientWriter.WriteAsync(byte[] data, CancellationToken ct)
        {
            if (_disconnected)
                return;

            try
            {
                await _client.GetStream().WriteAsync(data, 0, data.Length, ct);
            }
            catch (SocketException)
            {
                HandleDisconnection();
            }
        }

        #endregion


        #region IClientReader

        async Task<int> IClientReader.ReceiveDataAsync(byte[] buffer, int offset, int count, CancellationToken ct)
        {
            Guard.DebugAssertArgumentNotNull(buffer, nameof(buffer));
            Guard.DebugAssertArgument(offset >= 0, nameof(offset));
            Guard.DebugAssertArgument(count >= 0, nameof(count));

            if (_disconnected)
                return 0;

            try
            {
                return await _client.GetStream().ReadAsync(buffer, offset, count, ct);
            }
            catch (SocketException)
            {
                HandleDisconnection();
            }

            return 0;
        }

        #endregion
    }
}