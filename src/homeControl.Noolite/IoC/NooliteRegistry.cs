using System;
using homeControl.Noolite.Adapters;
using homeControl.Peripherals;
using StructureMap;

namespace homeControl.Noolite.IoC
{
    public sealed class NooliteRegistry : Registry
    {
        public NooliteRegistry()
        {
            For<IPC11XXAdapter>().Use<PC11XXAdapterWrapper>().Singleton();
            For<IRX2164Adapter>().Use<RX2164AdapterWrapper>().Singleton();
            For<ISwitchController>().Add<NooliteSwitchController>().Singleton();
        }
    }
}
