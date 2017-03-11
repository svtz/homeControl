using System.Net.Sockets;

namespace homeControl.ClientApi.Server
{
    internal interface IClientProcessorFactory
    {
        IClientProcessor Create(TcpClient client);
    }
}