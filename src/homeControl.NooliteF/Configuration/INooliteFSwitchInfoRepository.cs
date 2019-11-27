using System.Collections.Generic;
using System.Threading.Tasks;
using homeControl.Domain;

namespace homeControl.NooliteF.Configuration
{
    internal interface INooliteFSwitchInfoRepository
    {
        Task<bool> ContainsConfig(SwitchId switchId);
        Task<NooliteFSwitchInfo> GetConfig(SwitchId switchId);
        Task<IReadOnlyCollection<NooliteFSwitchInfo>> GetAll();
    }
}