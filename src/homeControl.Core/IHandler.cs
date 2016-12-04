using System;

namespace homeControl.Core
{
    public interface IHandler
    {
        bool CanHandle(IMessage message);
        IMessage[] Handle(IMessage message);
    }
}
