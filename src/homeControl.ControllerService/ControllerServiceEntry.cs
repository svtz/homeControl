using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using homeControl.Configuration.IoC;
using homeControl.ControllerService.Bindings;
using homeControl.ControllerService.Sensors;
using homeControl.Domain.Events.Bindings;
using homeControl.Domain.Events.Configuration;
using homeControl.Domain.Events.Sensors;
using homeControl.Domain.Events.Switches;
using homeControl.Entry;
using homeControl.Interop.Rabbit.IoC;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Serilog;
using Serilog.Events;
using StructureMap;

namespace homeControl.ControllerService
{
    internal sealed class ControllerService : AbstractService
    {
        protected override string ServiceNamePrefix => "controller";

        protected override IEnumerable<Registry> GetConfigurationRegistries(string uniqueServiceName)
        {
            yield return new RabbitConfigurationRegistryBuilder(Config)
                .UseJsonSerializationWithEncoding(Encoding.UTF8)
                .SetupEventSender<ConfigurationRequestEvent>("configuration-requests")
                .SetupEventSource<ConfigurationResponseEvent>("configuration", ExchangeType.Direct, uniqueServiceName)
                .SetupEventSource<AbstractBindingEvent>("main", ExchangeType.Fanout, "")
                .SetupEventSource<AbstractSensorEvent>("main", ExchangeType.Fanout, "")
                .SetupEventSender<AbstractSwitchEvent>("main")
                .Build();
            
            yield return new ConfigurationRegistry(uniqueServiceName);
            yield return new ControllerRegistry();
        }

        protected override void Run(IContainer workContainer, CancellationToken ct)
        {
            var bindingsProcessor = workContainer.GetInstance<BindingEventsProcessor>();
            var sensorsProcessor = workContainer.GetInstance<SensorEventsProcessor>();

            Task.WaitAll(
                bindingsProcessor.Run(ct),
                sensorsProcessor.Run(ct));
        }

        private static void Main(string[] args)
        {
            new ControllerService().Run();
        }
    }
}