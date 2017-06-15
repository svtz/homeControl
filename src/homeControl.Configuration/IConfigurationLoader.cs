using System.Threading.Tasks;

namespace homeControl.Configuration
{
    public interface IConfigurationLoader<TConfiguration>
    {
        Task<TConfiguration> Load(string configKey);
    }
}