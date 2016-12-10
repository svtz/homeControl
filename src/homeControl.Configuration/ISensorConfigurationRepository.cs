namespace homeControl.Configuration
{
    public interface ISensorConfigurationRepository
    {
        TSensorConfig[] GetAllConfigs<TSensorConfig>() where TSensorConfig : ISensorConfiguration;
    }
}