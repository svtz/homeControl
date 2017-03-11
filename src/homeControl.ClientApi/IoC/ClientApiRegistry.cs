using homeControl.ClientApi.Configuration;
using homeControl.ClientApi.Controllers;
using homeControl.ClientApi.Server;
using StructureMap;

namespace homeControl.ClientApi.IoC
{
    public class ClientApiRegistry : Registry
    {
        public ClientApiRegistry()
        {
            For<IClientApiConfigurationRepository>().Use<ClientApiConfigurationRepository>().Singleton();
            For<ISetSwitchValueStrategy>().Add<SetGradientSwitchValueStrategy>().Singleton();
            For<ISetSwitchValueStrategy>().Add<SetToggleSwitchValueStrategy>().Singleton();

            For<IClientListener>().Use<ClientListener>().Singleton();
            For<IClientsPool>().Use<ClientsPool>().Singleton();
            For<IClientProcessorFactory>().Use<ClientProcessorFactory>().Singleton();
        }
    }
}
