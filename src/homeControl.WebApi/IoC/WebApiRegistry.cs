using homeControl.WebApi.Configuration;
using homeControl.WebApi.Controllers;
using StructureMap;

namespace homeControl.WebApi.IoC
{
    public class WebApiRegistry : Registry
    {
        public WebApiRegistry()
        {
            For<IClientApiConfigurationRepository>().Use<JsonClientApiConfigurationRepository>().Singleton();
            For<ISetSwitchValueStrategy>().Add<SetGradientSwitchValueStrategy>().Singleton();
            For<ISetSwitchValueStrategy>().Add<SetToggleSwitchValueStrategy>().Singleton();
        }
    }
}
