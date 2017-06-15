using System;
using JetBrains.Annotations;
using RabbitMQ.Client;

namespace homeControl.ConfigurationStore
{
    [UsedImplicitly]
    internal sealed class ConnectionFactory
    {
        private readonly RabbitMQ.Client.ConnectionFactory _factory;

        public ConnectionFactory(Uri uri)
        {
            Guard.DebugAssertArgumentNotNull(uri, nameof(uri));

            _factory = new RabbitMQ.Client.ConnectionFactory
            {
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
                Uri = uri,
            };
        }

        public IConnection CreateConnection()
        {
            return _factory.CreateConnection();
        }
    }
}