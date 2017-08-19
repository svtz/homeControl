using System;
using System.Collections.Generic;
using System.Text;
using homeControl.Domain.Events;
using RabbitMQ.Client;
using StructureMap;

namespace homeControl.Interop.Rabbit.IoC
{
    public sealed class RabbitConfigurationRegistryBuilder
    {
        private readonly List<Action<Registry>> _registryConfigActions = new List<Action<Registry>>();
        private readonly List<Action<ExchangeConfiguration>> _exchangeConfigActions = new List<Action<ExchangeConfiguration>>();

        public RabbitConfigurationRegistryBuilder(string uri)
        {
            Guard.DebugAssertArgumentNotNull(uri, nameof(uri));

            _registryConfigActions.Add(cfg =>
            {
                cfg.ForConcreteType<ConnectionFactory>()
                    .Configure
                    .Ctor<Uri>("uri")
                    .Is(new Uri(uri))
                    .Singleton();
                cfg.ForConcreteType<Bus>()
                    .Configure
                    .Transient();
                cfg.Forward<Bus, IEventSource>();
                cfg.Forward<Bus, IEventSender>();
                cfg.For<IConnection>()
                    .Use(c => c.GetInstance<ConnectionFactory>().CreateConnection())
                    .Transient();
                cfg.For<IModel>()
                    .Use(c => c.GetInstance<IConnection>().CreateModel())
                    .Transient();
                cfg.For<IEventProcessorFactory>()
                    .Use<RabbitEventProcessorFactory>()
                    .Transient();
            });
        }

        public RabbitConfigurationRegistryBuilder UseJsonSerializationWithEncoding(Encoding encoding)
        {
            _registryConfigActions.Add(cfg => 
                cfg.For<IEventSerializer>()
                .Use<JsonEventSerializer>()
                .Ctor<Encoding>("encoding").Is(encoding)
                .Singleton());

            return this;
        }

        public RabbitConfigurationRegistryBuilder SetupEventSender<TEvent>(string exchangeName)
            where TEvent : IEvent
        {
            _exchangeConfigActions.Add(efg => efg.ConfigureEventSender(typeof(TEvent), exchangeName));

            return this;
        }


        public RabbitConfigurationRegistryBuilder SetupEventSource<TEvent>(string exchangeName, string exchangeType, string routingKey)
            where TEvent : IEvent
        {
            _exchangeConfigActions.Add(efg => efg.ConfigureEventSource(typeof(TEvent), exchangeName, exchangeType, routingKey));

            return this;
        }

        public Registry Build()
        {
            _registryConfigActions.Add(cfg =>
                cfg.ForConcreteType<ExchangeConfiguration>()
                    .Configure
                    .OnCreation(efg => _exchangeConfigActions.ForEach(action => action(efg)))
                    .Singleton());

            return new RabbitConfigurationRegistry(_registryConfigActions);
        }
    }
}