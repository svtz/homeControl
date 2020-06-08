namespace homeControl.Domain.Events
{
    public class NeedStatusEvent : IEvent
    {
        public override string ToString()
        {
            return nameof(NeedStatusEvent);
        }
    }
}