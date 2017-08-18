using System.Collections.Generic;
using System.Threading.Tasks;

namespace homeControl.Domain.Repositories
{
    public interface ISensorConfigurationRepository
    {
        Task<IReadOnlyCollection<TConfig>> GetAll<TConfig>() where TConfig : ISensorConfiguration;
    }
}