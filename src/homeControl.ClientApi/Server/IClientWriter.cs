using System.Threading.Tasks;

namespace homeControl.ClientApi.Server
{
    internal interface IClientWriter
    {
        Task WriteAsync(byte[] data);
    }
}