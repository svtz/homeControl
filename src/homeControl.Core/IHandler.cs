using System;

namespace homeControl.Core
{
    public interface IHandler
    {
        bool CanHandle(IMessage message);
        void Handle(IMessage message);
    }
}
