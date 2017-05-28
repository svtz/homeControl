using System.Net.Sockets;
using System.Threading;

namespace homeControl.ClientApi.Server
{
    internal sealed class ClientProcessorFactory : IClientProcessorFactory
    {
        private readonly IClientMessageSerializer _msgSerializer;
        private readonly IClientRequestRouter _clientRequestRouter;
        private readonly CancellationToken _ct;

        public ClientProcessorFactory(IClientMessageSerializer msgSerializer, IClientRequestRouter clientRequestRouter, CancellationToken ct)
        {
            Guard.DebugAssertArgumentNotNull(msgSerializer, nameof(msgSerializer));
            Guard.DebugAssertArgumentNotNull(clientRequestRouter, nameof(clientRequestRouter));

            _msgSerializer = msgSerializer;
            _clientRequestRouter = clientRequestRouter;
            _ct = ct;
        }

        public IClientProcessor Create(TcpClient client)
        {
            Guard.DebugAssertArgumentNotNull(client, nameof(client));

            return new ClientProcessor(client, _msgSerializer, _clientRequestRouter, _ct);
        }
    }
}