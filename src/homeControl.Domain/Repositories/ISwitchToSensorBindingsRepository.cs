using System.Collections.Generic;

namespace homeControl.Configuration.Bindings
{
    public interface ISwitchToSensorBindingsRepository
    {
        IReadOnlyCollection<ISwitchToSensorBinding> GetAll();
    }
}