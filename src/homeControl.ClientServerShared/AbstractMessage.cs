using System;

namespace homeControl.ClientServerShared
{
    public abstract class AbstractMessage : IClientMessage
    {
        public Guid RequestId { get; }

        protected AbstractMessage(Guid id)
        {
            RequestId = id;
        }
    }
}