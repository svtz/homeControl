using System.Collections.Generic;
using System.Threading.Tasks;

namespace homeControl.NooliteService.Configuration
{
    internal interface INooliteSensorInfoRepository
    {
        Task<IReadOnlyCollection<NooliteSensorInfo>> GetAll();
    }
}