using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using homeControl.Configuration;
using homeControl.Configuration.Bindings;
using homeControl.Configuration.Sensors;
using homeControl.Configuration.Switches;
using homeControl.Events.Bindings.Configuration;
using homeControl.WebApi.Configuration;
using homeControl.WebApi.Dto;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace homeControl.Experiments
{
    class SampleClientApiConfigGenerator
    {
        internal sealed class SampleClientApiConfigurationRepository : IClientApiConfigurationRepository
        {
            private readonly ISwitchConfigurationRepository _switchConfigurationRepository;
            private readonly ISwitchToSensorBindingsRepository _switchToSensorBindingsRepository;

            private readonly Lazy<Dictionary<Guid, SwitchApiConfig>> _config;

            public SampleClientApiConfigurationRepository(
                ISwitchConfigurationRepository switchConfigurationRepository,
                ISwitchToSensorBindingsRepository switchToSensorBindingsRepository)
            {
                _switchConfigurationRepository = switchConfigurationRepository;
                _switchToSensorBindingsRepository = switchToSensorBindingsRepository;

                _config = new Lazy<Dictionary<Guid, SwitchApiConfig>>(LoadConfiguration);
            }

            private Dictionary<Guid, SwitchApiConfig> LoadConfiguration()
            {
                var automatedSwitchesDict = _switchToSensorBindingsRepository
                    .GetAll()
                    .ToDictionary(cfg => cfg.SwitchId);

                var allSwitches = _switchConfigurationRepository.GetAll();

                var configurationDictionary = new Dictionary<Guid, SwitchApiConfig>(allSwitches.Count);
                foreach (var @switch in allSwitches)
                {
                    ISwitchToSensorBinding binding;
                    var config = automatedSwitchesDict.TryGetValue(@switch.SwitchId, out binding)
                        ? new AutomatedSwitchApiConfig { SensorId = binding.SensorId }
                        : new SwitchApiConfig();

                    config.ConfigId = Guid.NewGuid();
                    config.Name = config.ConfigId.ToString();
                    config.Description = config.ConfigId.ToString();
                    config.Kind = SwitchKind.GradientSwitch;
                    config.SwitchId = @switch.SwitchId;

                    configurationDictionary.Add(config.ConfigId, config);
                }

                // мысль: один датчик влияет на два переключателя, и я хочу отключить только один. Получится?
                return configurationDictionary;
            }

            public IReadOnlyCollection<SwitchApiConfig> GetAll()
            {
                return _config.Value.Values;
            }

            public SwitchApiConfig TryGetById(Guid id)
            {
                var configurationDictionary = _config.Value;

                SwitchApiConfig config;
                return configurationDictionary.TryGetValue(id, out config) ? config : null;
            }
        }

        public void Run()
        {
            var configPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "..\\homeControl.Application\\conf");
            var converters = new JsonConverter[]
            {
                new SwitchIdSerializer(),
                new SensorIdSerializer(),
                new SwitchToSensorBindingSerializer()
            };
            var config = new SampleClientApiConfigurationRepository(
                new SwitchConfgurationRepository(new JsonConfigurationLoader<ISwitchConfiguration[]>(configPath, converters)),
                new SwitchToSensorBindingsRepository(new JsonConfigurationLoader<ISwitchToSensorBinding[]>(configPath, converters)));

            var configString = JsonConvert.SerializeObject(config.GetAll(), new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = new JsonConverter[] { new StringEnumConverter() }
            });

            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "client-api.json"), configString);
        }
    }

}