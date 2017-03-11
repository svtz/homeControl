using System;

namespace homeControl.WebApi.Server
{
    internal interface IClientProcessor
    {
        void Start();
        event EventHandler Disconnected;
    }
}