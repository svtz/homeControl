using Microsoft.Extensions.Configuration;

namespace homeControl.Entry
{
    internal static class ConfigHolder
    {
        public static IConfigurationRoot Config { get; }
            = new ConfigurationBuilder()
                .AddJsonFile("settings.json")
                .Build();
    }
}