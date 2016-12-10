using System;
using StructureMap;

namespace homeControl.Peripherals.IoC
{
    public sealed class PeripheralsRegistry : Registry
    {
        public PeripheralsRegistry()
        {
            For<ISwitchControllerSelector>().Use<SwitchControllerSelector>();
        }
    }
}
