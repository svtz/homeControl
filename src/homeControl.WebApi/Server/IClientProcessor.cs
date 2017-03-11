using System;

namespace homeControl.WebApi.Server
{
    internal interface IClientProcessor: IDisposable
    {
        void Start();
        void Stop();
        event EventHandler Disconnected;
    }
}