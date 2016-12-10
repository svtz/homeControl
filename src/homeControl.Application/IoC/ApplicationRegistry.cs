using System;
using System.IO;
using homeControl.Configuration.IoC;
using homeControl.Core.IoC;
using homeControl.Events.IoC;
using homeControl.Noolite.IoC;
using homeControl.Peripherals;
using homeControl.Peripherals.IoC;
using StructureMap;

namespace homeControl.Application.IoC
{
    public class ApplicationRegistry : Registry
    {
        public ApplicationRegistry()
        {
            var workdir = Directory.GetCurrentDirectory();
            var configPath = Path.Combine(workdir, "config.json");

            IncludeRegistry<CoreRegistry>();
            IncludeRegistry<EventsRegistry>();
            IncludeRegistry(new ConfigurationRegistry(configPath));
            IncludeRegistry<NooliteRegistry>();
            IncludeRegistry<PeripheralsRegistry>();

            For<ISwitchController>().Add<SwitchControllerConsoleEmulator>().Singleton();
        }
    }
}
