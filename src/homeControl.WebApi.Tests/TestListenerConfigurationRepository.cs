using System.Net;
using homeControl.WebApi.Configuration;

namespace homeControl.WebApi.Tests
{
    internal sealed class TestListenerConfigurationRepository : IClientListenerConfigurationRepository
    {
        public static readonly IPAddress IPAddress = IPAddress.Loopback;
        public static readonly int PortNumber = TcpTestsHelper.PortNumber;

        public ClientListenerConfiguration Get()
        {
            var config = new ClientListenerConfiguration
            {
                IPAddress = IPAddress.ToString(),
                Port = PortNumber
            };

            return config;
        }
    }
}