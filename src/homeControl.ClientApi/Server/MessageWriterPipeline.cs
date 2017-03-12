using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using homeControl.ClientServerShared;

namespace homeControl.ClientApi.Server
{
    internal sealed class MessageWriterPipeline : IDisposable, IMessageWriterPipeline
    {
        private readonly IClientsWriter _writer;
        private readonly IClientMessageSerializer _serializer;
        private readonly ITargetBlock<IClientMessage> _pipeline;
        private readonly CancellationTokenSource _cts;

        public MessageWriterPipeline(IClientsWriter writer, IClientMessageSerializer serializer)
        {
            Guard.DebugAssertArgumentNotNull(writer, nameof(writer));
            Guard.DebugAssertArgumentNotNull(serializer, nameof(serializer));

            _writer = writer;
            _serializer = serializer;
            _cts = new CancellationTokenSource();
            _pipeline = BuildPipeline();
        }

        private ITargetBlock<IClientMessage> BuildPipeline()
        {
            const int capacity = 10;
            var bufferOptions = new DataflowBlockOptions
            {
                EnsureOrdered = false,
                CancellationToken = _cts.Token,
                BoundedCapacity = capacity
            };
            var executionOptions = new ExecutionDataflowBlockOptions
            {
                CancellationToken = _cts.Token,
                BoundedCapacity = capacity,
                EnsureOrdered = false,
                MaxDegreeOfParallelism = 1
            };

            var input = new BufferBlock<IClientMessage>(bufferOptions);
            var serializerBlock = new TransformBlock<IClientMessage, byte[]>(m => PerformSerialization(m), executionOptions);
            var weighterBlock = new TransformBlock<byte[], byte[]>(m => AddLength(m), executionOptions);
            var writerBlock = new ActionBlock<byte[]>(WriteMessage, executionOptions);

            input.LinkTo(serializerBlock);
            serializerBlock.LinkTo(weighterBlock);
            weighterBlock.LinkTo(writerBlock);

            return input;
        }

        private Task WriteMessage(byte[] data)
        {
            Guard.DebugAssertArgumentNotNull(data, nameof(data));

            return _writer.WriteAsync(data);
        }

        private byte[] AddLength(byte[] data)
        {
            Guard.DebugAssertArgumentNotNull(data, nameof(data));

            var length = data.Length;
            return BitConverter.GetBytes(length).Concat(data).ToArray();
        }

        private byte[] PerformSerialization(IClientMessage message)
        {
            Guard.DebugAssertArgumentNotNull(message, nameof(message));

            return _serializer.Serialize(message);
        }

        public void PostMessage(IClientMessage message)
        {
            Guard.DebugAssertArgumentNotNull(message, nameof(message));
            CheckNotDisposed();

            _pipeline.Post(message);
        }

        private bool _disposed = false;
        private void CheckNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }


        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _cts.Cancel();
            _cts.Dispose();
            _disposed = true;
        }
    }
}
