using System.Linq;
using System.Threading.Tasks;

namespace homeControl.ClientApi.Server
{
    internal sealed class ClientsBroadcaster : IClientWriter
    {
        private readonly IClientsPool _pool;

        public ClientsBroadcaster(IClientsPool pool)
        {
            _pool = pool;
        }

        public Task WriteAsync(byte[] data)
        {
            var writerTasks = _pool.GetAll()
                .OfType<IClientWriter>()
                .Select(cli => cli.WriteAsync(data));

            return Task.WhenAll(writerTasks);
        }
    }
}