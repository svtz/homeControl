using homeControl.WebApi.Configuration;
using homeControl.WebApi.Controllers;
using homeControl.WebApi.Server;
using StructureMap;

namespace homeControl.WebApi.IoC
{
    public class WebApiRegistry : Registry
    {
        public WebApiRegistry()
        {
            For<IClientApiConfigurationRepository>().Use<ClientApiConfigurationRepository>().Singleton();
            For<ISetSwitchValueStrategy>().Add<SetGradientSwitchValueStrategy>().Singleton();
            For<ISetSwitchValueStrategy>().Add<SetToggleSwitchValueStrategy>().Singleton();

            For<IClientListener>().Use<ClientListener>().Singleton();
        }
    }
}
