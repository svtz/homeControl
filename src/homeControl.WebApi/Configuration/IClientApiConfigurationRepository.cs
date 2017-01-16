namespace homeControl.WebApi.Configuration
{
    public interface IClientApiConfigurationRepository
    {
        SwitchApiConfig[] GetClientApiConfig();
    }
}