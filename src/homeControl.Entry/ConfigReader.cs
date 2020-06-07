using System;
using Microsoft.Extensions.Configuration;

namespace homeControl.Entry
{
    public class ConfigReader
    {
        public IConfigurationRoot ReadConfig()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("settings.json");

#if DEBUG
                builder.AddJsonFile("settings.Debug.json");
#else
                builder.AddJsonFile($"settings.Production.json");
#endif
            
            return builder.Build();
        }
    }
}