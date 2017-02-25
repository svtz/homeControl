using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using homeControl.Configuration;
using Newtonsoft.Json;

namespace homeControl.WebApi.Configuration
{
    internal sealed class JsonClientApiConfigurationRepository : IClientApiConfigurationRepository
    {
        private readonly Lazy<Dictionary<Guid, SwitchApiConfig>> _config;

        public JsonClientApiConfigurationRepository()
        {
            _config = new Lazy<Dictionary<Guid, SwitchApiConfig>>(LoadConfiguration);
        }

        private static Dictionary<Guid, SwitchApiConfig> LoadConfiguration()
        {
            try
            {
                var configText = File.ReadAllText("client-api.json");
                var config = JsonConvert.DeserializeObject<SwitchApiConfig[]>(configText);
                return config.ToDictionary(c => c.ConfigId);
            }
            catch (Exception ex)
            {
                throw new InvalidConfigurationException(ex, "Error while clientApi configuration loading.");
            }
        }

        public SwitchApiConfig[] GetAll()
        {
            return _config.Value.Values.ToArray();
        }

        public SwitchApiConfig TryGetById(Guid id)
        {
            var configurationDictionary = _config.Value;

            SwitchApiConfig config;
            return configurationDictionary.TryGetValue(id, out config) ? config : null;
        }
    }
}