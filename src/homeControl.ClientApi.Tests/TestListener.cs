using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace homeControl.ClientApi.Tests
{
    internal sealed class TestListener : IDisposable
    {
        private readonly KeyValuePair<byte[], byte[]>[] _requestAnswerMap;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private TcpClient _client;

        public TestListener(IEnumerable<KeyValuePair<string, string>> requestAnswerMap)
        {
            Guard.DebugAssertArgumentNotNull(_requestAnswerMap, nameof(_requestAnswerMap));

            var utf8 = Encoding.UTF8;
            _requestAnswerMap = requestAnswerMap
                .Select(kv => new KeyValuePair<byte[], byte[]>(utf8.GetBytes(kv.Key), utf8.GetBytes(kv.Value)))
                .ToArray();
        }

        private int MaxRequestLength()
        {
            return _requestAnswerMap.Max(kv => kv.Key.Length);
        }

        public void Start()
        {
            Task.Factory.StartNew(() =>
            {
                var listener = new TcpListener(IPAddress.Loopback, TcpTestsHelper.PortNumber);
                var connectTask = listener.AcceptTcpClientAsync();
                connectTask.Wait(_cts.Token);

                _client = connectTask.Result;

                var stream = _client.GetStream();
                var buffer = new byte[MaxRequestLength()];

                while (true)
                {
                    _cts.Token.ThrowIfCancellationRequested();

                    var readTask = stream.ReadAsync(buffer, 0, buffer.Length, _cts.Token);
                    var requestLength = readTask.Result;

                    var request = buffer.Take(requestLength);
                    var answer = _requestAnswerMap.Single(kv => request.SequenceEqual(kv.Key)).Value;
                    stream.Write(answer, 0, answer.Length);
                }
            }, _cts.Token);
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
            _client.Dispose();
        }
    }
}