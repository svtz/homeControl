namespace homeControl.Configuration
{
    public interface ISensorConfigurationRepository
    {
        TSensorConfig[] GetAllConfigs<TSensorConfig>();
    }
}