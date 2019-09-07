using homeControl.Configuration;
using homeControl.NooliteService.Adapters;
using homeControl.NooliteService.Configuration;
using homeControl.NooliteService.SwitchController;
using StructureMap;

namespace homeControl.NooliteService
{
    internal class NooliteRegistry : Registry
    {
        public NooliteRegistry()
        {
            For<IPC11XXAdapter>().Use<PC11XXAdapterWrapper>().Singleton();
            For<IRX2164Adapter>().Use<RX2164AdapterWrapper>().Singleton();
            For<ISwitchController>().Add<NooliteSwitchController>().Singleton();
            For<INooliteSwitchInfoRepository>().Use<NooliteSwitchInfoRepository>().Transient();
            For<INooliteSensorInfoRepository>().Use<NooliteSensorInfoRepository>().Transient();
            ForConcreteType<NooliteSensor>().Configure.Transient();
        }
    }
}