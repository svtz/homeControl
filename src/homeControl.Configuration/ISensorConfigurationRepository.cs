namespace homeControl.Configuration
{
    public interface ISensorConfigurationRepository
    {
        TSensorConfig[] GetAllSensorConfigs<TSensorConfig>();
    }
}