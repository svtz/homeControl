using System.Collections.Generic;

namespace homeControl.ClientApi.Server
{
    internal sealed class ClientsPool : IClientsPool
    {
        private readonly List<IClientProcessor> _clients = new List<IClientProcessor>();

        public void Add(IClientProcessor client)
        {
            Guard.DebugAssertArgumentNotNull(client, nameof(client));

            lock (_clients)
                _clients.Add(client);
        }

        public void Remove(IClientProcessor client)
        {
            Guard.DebugAssertArgumentNotNull(client, nameof(client));

            lock (_clients)
            {
                Guard.DebugAssertArgument(_clients.Contains(client), nameof(client));
                _clients.Remove(client);
            }
        }
    }
}