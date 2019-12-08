using System.Collections.Generic;
using System.Threading.Tasks;
using homeControl.Domain.Configuration;

namespace homeControl.Domain.Repositories
{
    public interface ISensorConfigurationRepository
    {
        Task<IReadOnlyCollection<SensorConfiguration>> GetAll();
    }
}