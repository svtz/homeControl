using homeControl.NooliteF.Adapters;
using homeControl.NooliteF.Configuration;
using homeControl.NooliteF.SwitchController;
using Microsoft.Extensions.Configuration;
using Serilog;
using StructureMap;

namespace homeControl.NooliteF
{
    internal class NooliteFRegistry : Registry
    {
        public NooliteFRegistry(IConfigurationRoot config)
        {
            For<IMtrfAdapter>()
                .Use(sp => new AdapterWrapper(config["AdapterPort"], sp.GetInstance<ILogger>()))
                .Singleton();
            For<ISwitchController>().Add<NooliteFSwitchController>().Singleton();
            For<INooliteFSwitchInfoRepository>().Use<NooliteFSwitchInfoRepository>().Transient();
            For<INooliteFSensorInfoRepository>().Use<NooliteFSensorInfoRepository>().Transient();
            ForConcreteType<NooliteFSensor>().Configure.Transient();
        }
    }
}