using System.Collections.Generic;
using System.Text;
using System.Threading;
using homeControl.Configuration.IoC;
using homeControl.Domain.Events.Configuration;
using homeControl.Domain.Events.Sensors;
using homeControl.Domain.Events.Switches;
using homeControl.Entry;
using homeControl.Interop.Rabbit.IoC;
using RabbitMQ.Client;
using Serilog;
using StructureMap;

namespace homeControl.NooliteService
{
    public class NooliteService : AbstractService
    {
        protected override string ServiceNamePrefix { get; } = "noolite";
        
        protected override IEnumerable<Registry> GetConfigurationRegistries(string uniqueServiceName)
        {
            yield return new RabbitConfigurationRegistryBuilder(Config)
                .UseJsonSerializationWithEncoding(Encoding.UTF8)
                .SetupEventSender<ConfigurationRequestEvent>("configuration-requests")
                .SetupEventSource<ConfigurationResponseEvent>("configuration", ExchangeType.Direct, uniqueServiceName)
                .SetupEventSender<AbstractSensorEvent>("main")
                .SetupEventSource<AbstractSwitchEvent>("main", ExchangeType.Fanout, "")
                .Build();
            
            yield return new ConfigurationRegistry(uniqueServiceName);
            yield return new NooliteRegistry();
        }

        protected override void Run(IContainer container, CancellationToken ct)
        {
            Logger.Debug("Run: the beginning");
            
            container.GetInstance<NooliteSensor>().Activate();
            
            var switchesProcessor = container.GetInstance<SwitchEventsProcessor>();
 
            Logger.Debug("Starting SwitchEventsProcessor");
            switchesProcessor.RunAsync(ct);
            switchesProcessor.Completion(ct).Wait(ct);
        }
        
        private static void Main(string[] args)
        {
            new NooliteService().Run();
        }
    }
}