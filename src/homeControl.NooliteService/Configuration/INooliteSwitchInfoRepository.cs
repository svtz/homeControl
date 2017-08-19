using System.Collections.Generic;
using System.Threading.Tasks;
using homeControl.Domain;

namespace homeControl.NooliteService.Configuration
{
    internal interface INooliteSwitchInfoRepository
    {
        Task<bool> ContainsConfig(SwitchId switchId);
        Task<NooliteSwitchInfo> GetConfig(SwitchId switchId);
        Task<IReadOnlyCollection<NooliteSwitchInfo>> GetAll();
    }
}