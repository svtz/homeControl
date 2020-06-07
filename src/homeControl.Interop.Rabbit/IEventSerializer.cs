using homeControl.Domain.Events;

namespace homeControl.Interop.Rabbit
{
    internal interface IEventSerializer
    {
        IEvent Deserialize(byte[] messageBytes);
        byte[] Serialize(IEvent message);
    }
}