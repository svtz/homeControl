using System;
using System.Net.Sockets;

namespace homeControl.ClientApi.Server
{
    internal interface IClientProcessor
    {
        void Start();
        void Stop();
        event EventHandler Disconnected;
    }
}