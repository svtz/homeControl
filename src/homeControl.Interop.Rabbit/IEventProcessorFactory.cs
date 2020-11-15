using homeControl.Domain.Events;

namespace homeControl.Interop.Rabbit
{
    internal interface IEventProcessorFactory
    {
        IEventReceiver CreateReceiver(string exchangeName, string exchangeType, string routingKey);
        IEventSender CreateSender(string exchangeName);
    }
}