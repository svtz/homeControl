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

            var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");

            if (string.IsNullOrWhiteSpace(environment))
            {
                builder.AddJsonFile("settings.Debug.json");
            }
            else
            {
                builder.AddJsonFile($"settings.{environment}.json");
            }

            return builder.Build();
        }
    }
}