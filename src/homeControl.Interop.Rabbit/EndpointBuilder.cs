using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using homeControl.Domain.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using NServiceBus.Faults;
using Serilog;
using IEvent = homeControl.Domain.Events.IEvent;

namespace homeControl.Interop.Rabbit
{
    public sealed class EndpointBuilder
    {
        private readonly IConfigurationRoot _rootConfig;
        private readonly ILogger _logger;
        private readonly List<Action<IServiceCollection>> _serviceConfigActions = new List<Action<IServiceCollection>>();

        private string _endpointName = null;
        
        public EndpointBuilder(IConfigurationRoot config, ILogger logger)
        {
            Guard.DebugAssertArgumentNotNull(config, nameof(config));
            _rootConfig = config;
            _logger = logger;

            _serviceConfigActions.Add(services =>
            {
                services.AddSingleton<IEventSource>(new EventSource(_logger));
                services.AddTransient<IEventSender, EventSender>();
            });
        }

        public EndpointBuilder UseEndpointName(string endpointName)
        {
            Guard.DebugAssertArgumentNotNull(endpointName, nameof(endpointName));
            if (_endpointName != null)
                throw new InvalidOperationException("Endpoint name already set");

            _endpointName = endpointName;
            return this;
        }

        public async Task<IEndpointInstance> Build(IServiceCollection services)
        {
            if (string.IsNullOrWhiteSpace(_endpointName))
                throw new InvalidOperationException("Endpoint name is not specified");
            
            var endpointConfiguration = new EndpointConfiguration(_endpointName);
            endpointConfiguration.LicensePath(Path.Combine("license", "License.xml"));
            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString($@"host={_rootConfig["RabbitHost"]};
                                          username={_rootConfig["RabbitUserName"]};
                                          password={_rootConfig["RabbitUserPass"]};
                                          virtualHost=homeControl");
            transport.UseConventionalRoutingTopology();
            transport.Transactions(TransportTransactionMode.ReceiveOnly);
            
            endpointConfiguration.Notifications.Errors.MessageSentToErrorQueue += ErrorsOnMessageSentToErrorQueue;

            endpointConfiguration.AssemblyScanner()
                .ScanAppDomainAssemblies = true;
            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.EnableInstallers();

            endpointConfiguration.Conventions()
                .DefiningEventsAs(e => typeof(IEvent).IsAssignableFrom(e));

            foreach (var configAction in _serviceConfigActions)
            {
                configAction(services);
            }
            endpointConfiguration.UseContainer<ServicesBuilder>(b => b.ExistingServices(services));
            var endpoint = await Endpoint.Start(endpointConfiguration);
            services.AddSingleton(endpoint);

            return endpoint;
        }

        private void ErrorsOnMessageSentToErrorQueue(object sender, FailedMessage e)
        {
            _logger.Fatal(e.Exception, $"Failed to process message with Id {e.MessageId}: {e.Exception.Message}");
        }
    }
}