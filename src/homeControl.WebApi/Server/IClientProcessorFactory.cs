using System.Net.Sockets;

namespace homeControl.WebApi.Server
{
    internal interface IClientProcessorFactory
    {
        IClientProcessor Create(TcpClient client);
    }
}