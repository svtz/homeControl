using System.Threading.Tasks;

namespace homeControl.ClientApi.Server
{
    internal interface IClientsWriter
    {
        Task WriteAsync(byte[] data);
    }
}