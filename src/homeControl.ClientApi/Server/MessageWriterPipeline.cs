using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using homeControl.ClientServerShared;

namespace homeControl.ClientApi.Server
{
    internal sealed class MessageWriterPipeline : IDisposable
    {
        private readonly IClientWriter _writer;
        private readonly IClientMessageSerializer _serializer;
        private readonly ITargetBlock<IClientMessage> _pipeline;
        private readonly CancellationToken _ct;

        public MessageWriterPipeline(IClientWriter writer, IClientMessageSerializer serializer, CancellationToken ct)
        {
            Guard.DebugAssertArgumentNotNull(writer, nameof(writer));
            Guard.DebugAssertArgumentNotNull(serializer, nameof(serializer));

            _writer = writer;
            _serializer = serializer;
            _ct = ct;
            _pipeline = BuildPipeline();
        }

        private ITargetBlock<IClientMessage> BuildPipeline()
        {
            var bufferOptions = new DataflowBlockOptions
            {
                EnsureOrdered = false,
                CancellationToken = _ct,
            };
            var executionOptions = new ExecutionDataflowBlockOptions
            {
                CancellationToken = _ct,
                EnsureOrdered = true,
                MaxDegreeOfParallelism = 1,
                SingleProducerConstrained = true
            };

            var input = new BufferBlock<IClientMessage>(bufferOptions);
            var serializerBlock = new TransformBlock<IClientMessage, byte[]>(m => PerformSerialization(m), executionOptions);
            var weighterBlock = new TransformBlock<byte[], byte[]>(m => AddLength(m), executionOptions);
            var writerBlock = new ActionBlock<byte[]>(WriteMessage, executionOptions);

            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            input.LinkTo(serializerBlock, linkOptions);
            serializerBlock.LinkTo(weighterBlock, linkOptions);
            weighterBlock.LinkTo(writerBlock, linkOptions);

            return input;
        }

        private Task WriteMessage(byte[] data)
        {
            Guard.DebugAssertArgumentNotNull(data, nameof(data));

            return _ct.IsCancellationRequested 
                ? Task.CompletedTask 
                : _writer.WriteAsync(data, _ct);
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

            if (!_ct.IsCancellationRequested)
                _pipeline.SendAsync(message, _ct);
        }

        private bool _disposed = false;
        public void Dispose()
        {
            if (_disposed) return;

            _pipeline.Complete();
            _disposed = true;
        }
    }
}
