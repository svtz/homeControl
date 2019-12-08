using System.Collections.Generic;
using System.Threading.Tasks;
using homeControl.Domain.Configuration;

namespace homeControl.Domain.Repositories
{
    public interface ISwitchConfigurationRepository
    {
        Task<bool> ContainsConfig(SwitchId switchId);
        Task<SwitchConfiguration> GetConfig(SwitchId switchId);
        Task<IReadOnlyCollection<SwitchConfiguration>> GetAll();
    }
}