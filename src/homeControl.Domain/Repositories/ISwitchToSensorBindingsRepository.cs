using System.Collections.Generic;
using System.Threading.Tasks;

namespace homeControl.Domain.Repositories
{
    public interface ISwitchToSensorBindingsRepository
    {
        Task<IReadOnlyCollection<ISwitchToSensorBinding>> GetAll();
    }
}