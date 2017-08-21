using System.Linq;
using GalaSoft.MvvmLight;
using homeControl.Domain;
using Serilog;

namespace homeControl.SensorEmulator.ViewModels.Sensors
{
    public abstract class SensorViewModelBase : ViewModelBase
    {
        protected ILogger Log { get; }

        public SensorId Id { get; set; }
        public string Name => Id.Id.ToString("D").Split('-').First();

        protected SensorViewModelBase(ILogger log)
        {
            Guard.DebugAssertArgumentNotNull(log, nameof(log));
            Log = log.ForContext(GetType());
        }
    }
}