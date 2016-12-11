namespace homeControl.Configuration.Sensors
{
    public interface ISensorConfigurationRepository
    {
        TSensorConfig[] GetAllConfigs<TSensorConfig>() where TSensorConfig : ISensorConfiguration;
    }
}