using System.Collections.Generic;

namespace homeControl.Domain.Repositories
{
    public interface ISwitchToSensorBindingsRepository
    {
        IReadOnlyCollection<ISwitchToSensorBinding> GetAll();
    }
}