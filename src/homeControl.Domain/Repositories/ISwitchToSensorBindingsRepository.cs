using System.Collections.Generic;
using System.Threading.Tasks;
using homeControl.Domain.Configuration.Bindings;

namespace homeControl.Domain.Repositories
{
    public interface ISwitchToSensorBindingsRepository
    {
        Task<IReadOnlyCollection<SwitchToSensorBinding>> GetAll();
    }
}