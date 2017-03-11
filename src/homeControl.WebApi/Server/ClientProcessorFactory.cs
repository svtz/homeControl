using System.Net.Sockets;

namespace homeControl.ClientApi.Server
{
    internal sealed class ClientProcessorFactory : IClientProcessorFactory
    {
        public IClientProcessor Create(TcpClient client)
        {
            Guard.DebugAssertArgumentNotNull(client, nameof(client));

            return new ClientProcessor(client);
        }
    }
}