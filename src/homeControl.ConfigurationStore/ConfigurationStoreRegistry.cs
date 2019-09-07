using System.IO;
using StructureMap;

namespace homeControl.ConfigurationStore
{
    internal sealed class ConfigurationStoreRegistry : Registry
    {
        public ConfigurationStoreRegistry()
        {
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "conf");
            ForConcreteType<ConfigurationProvider>()
                .Configure
                .Ctor<string>("configurationsDirectory").Is(configPath);
            ForConcreteType<ConfigurationRequestsProcessor>();
        }
    }
}