using System.Net.Sockets;
using System.Threading;

namespace homeControl.ClientApi.Server
{
    internal sealed class ClientProcessorFactory : IClientProcessorFactory
    {
        private readonly IClientMessageSerializer _msgSerializer;
        private readonly CancellationToken _ct;

        public ClientProcessorFactory(IClientMessageSerializer msgSerializer, CancellationToken ct)
        {
            Guard.DebugAssertArgumentNotNull(msgSerializer, nameof(msgSerializer));

            _msgSerializer = msgSerializer;
            _ct = ct;
        }

        public IClientProcessor Create(TcpClient client)
        {
            Guard.DebugAssertArgumentNotNull(client, nameof(client));

            return new ClientProcessor(client, _msgSerializer, _ct);
        }
    }
}