using System.Collections.Generic;
using System.Threading.Tasks;

namespace homeControl.NooliteF.Configuration
{
    internal interface INooliteFSensorInfoRepository
    {
        Task<IReadOnlyCollection<NooliteFSensorInfo>> GetAll();
    }
}