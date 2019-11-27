using homeControl.NooliteF.Adapters;
using homeControl.NooliteF.Configuration;
using homeControl.NooliteF.SwitchController;
using StructureMap;

namespace homeControl.NooliteF
{
    internal class NooliteFRegistry : Registry
    {
        public NooliteFRegistry()
        {
            For<IMtrfAdapter>().Use<AdapterWrapper>().Singleton();
            For<ISwitchController>().Add<NooliteFSwitchController>().Singleton();
            For<INooliteFSwitchInfoRepository>().Use<NooliteFSwitchInfoRepository>().Transient();
            For<INooliteFSensorInfoRepository>().Use<NooliteFSensorInfoRepository>().Transient();
            ForConcreteType<NooliteFSensor>().Configure.Transient();
        }
    }
}