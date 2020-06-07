using homeControl.Domain.Events;

namespace homeControl.Interop.Rabbit
{
    internal interface IEventProcessorFactory
    {
        IEventSource CreateSource(string exchangeName, string exchangeType, string routingKey);
        IEventSender CreateSender(string exchangeName);
    }
}