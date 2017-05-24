using System;
using System.Threading;
using System.Threading.Tasks;

namespace homeControl.ClientApi.Server
{
    internal interface IClientReader
    {
        Task<int> ReceiveDataAsync(byte[] buffer, int offset, int count, CancellationToken ct);
    }
}