using System.Collections.Generic;
using System.Threading.Tasks;

namespace homeControl.Domain.Repositories
{
    public interface ISensorConfigurationRepository
    {
        Task<IReadOnlyCollection<SensorConfiguration>> GetAll();
    }
}