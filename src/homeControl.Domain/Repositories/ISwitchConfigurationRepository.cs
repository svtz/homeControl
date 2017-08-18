using System.Collections.Generic;
using System.Threading.Tasks;

namespace homeControl.Domain.Repositories
{
    public interface ISwitchConfigurationRepository
    {
        Task<bool> ContainsConfig<TConfig>(SwitchId switchId) where TConfig: ISwitchConfiguration;
        Task<TConfig> GetConfig<TConfig>(SwitchId switchId) where TConfig: ISwitchConfiguration;
        Task<IReadOnlyCollection<ISwitchConfiguration>> GetAll();
    }
}