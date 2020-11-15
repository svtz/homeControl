using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using homeControl.Domain.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Serilog;

namespace homeControl.Interop.Rabbit.IoC
{
    public sealed class RabbitConfiguration
    {
        private readonly List<Action<IServiceCollection>> _serviceConfigActions = new List<Action<IServiceCollection>>();
        private readonly List<Action<ExchangeConfiguration>> _exchangeConfigActions = new List<Action<ExchangeConfiguration>>();

        public RabbitConfiguration(IConfigurationRoot config)
        {
            Guard.DebugAssertArgumentNotNull(config, nameof(config));

            _serviceConfigActions.Add(services =>
            {
                services.AddSingleton(new ConnectionFactory()
                {
                    Uri = new Uri($"amqp://{config["RabbitUserName"]}:{config["RabbitUserPass"]}@{config["RabbitHost"]}")
                });
                services.AddSingleton<Bus>();
                services.AddTransient<IEventReceiver>(sp => sp.GetRequiredService<Bus>());
                services.AddTransient<IEventSender>(sp => sp.GetRequiredService<Bus>());
                services.AddSingleton<IConnection>(sp => sp.GetRequiredService<ConnectionFactory>().CreateConnection());
                services.AddSingleton<IModel>(sp => sp.GetRequiredService<IConnection>().CreateModel());
                services.AddTransient<IEventProcessorFactory, RabbitEventProcessorFactory>();
            });
        }

        public RabbitConfiguration UseJsonSerializationWithEncoding(Encoding encoding)
        {
            _serviceConfigActions.Add(services =>
                services.AddSingleton<IEventSerializer>(sp => new JsonEventSerializer(
                    encoding,
                    sp.GetServices<JsonConverter>().ToArray(),
                    sp.GetRequiredService<ILogger>())));

            return this;
        }

        public RabbitConfiguration SetupEventSender<TEvent>(string exchangeName)
            where TEvent : IEvent
        {
            _exchangeConfigActions.Add(efg => efg.ConfigureEventSender(typeof(TEvent), exchangeName));

            return this;
        }


        public RabbitConfiguration SetupEventReceiver<TEvent>(string exchangeName, string exchangeType, string routingKey)
            where TEvent : IEvent
        {
            _exchangeConfigActions.Add(efg => efg.ConfigureEventReceiver(typeof(TEvent), exchangeName, exchangeType, routingKey));

            return this;
        }

        public void Apply(IServiceCollection services)
        {
            services.AddSingleton(sp =>
            {
                var cfg = new ExchangeConfiguration();
                _exchangeConfigActions.ForEach(action => action(cfg));
                return cfg;
            });
            
            _serviceConfigActions.ForEach(cfg => cfg(services));
        }
    }
}