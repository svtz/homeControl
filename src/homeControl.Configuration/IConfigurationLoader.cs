namespace homeControl.Configuration
{
    public interface IConfigurationLoader<out TConfiguration>
    {
        TConfiguration Load(string fileName);
    }
}