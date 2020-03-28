using System.Threading.Tasks;

namespace homeControl.Domain.Events
{
    public interface IEventSender
    {
        Task SendEvent(IEvent @event);
    }
}