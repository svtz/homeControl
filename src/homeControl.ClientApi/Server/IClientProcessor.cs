using System;

namespace homeControl.ClientApi.Server
{
    internal interface IClientProcessor
    {
        void Start();
        event EventHandler Disconnected;
    }
}