using GalaSoft.MvvmLight;
using homeControl.Domain;
using Serilog;

namespace homeControl.Client.WPF.ViewModels.Switches
{
    public abstract class SwitchViewModelBase : ViewModelBase
    {
        protected ILogger Log { get; }

        public SwitchId Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsAutomated { get; set; }

        protected SwitchViewModelBase(ILogger log)
        {
            Guard.DebugAssertArgumentNotNull(log, nameof(log));
            Log = log.ForContext(GetType());
        }
    }
}