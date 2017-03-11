using System;
using homeControl.WebApi.Server;
using Moq;

namespace homeControl.WebApi.Tests.Mocks
{
    internal sealed class ClientsPoolMock : Mock<IClientsPool>
    {
        public ClientsPoolMock() : base(MockBehavior.Strict)
        {
        }

        public ClientsPoolMock CanAdd(IClientProcessor processor, Action callback = null)
        {
            var setup = Setup(m => m.Add(processor));
            if (callback != null)
                setup.Callback(callback);

            return this;
        }

        public ClientsPoolMock CanRemove(IClientProcessor processor, Action callback = null)
        {
            var setup = Setup(m => m.Remove(processor));
            if (callback != null)
                setup.Callback(callback);

            return this;
        }
    }
}