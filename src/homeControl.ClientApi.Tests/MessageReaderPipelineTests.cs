using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using homeControl.ClientApi.Server;
using homeControl.ClientServerShared;
using Moq;
using Xunit;

namespace homeControl.ClientApi.Tests
{
    public class MessageReaderPipelineTests
    {
        private const string MessageString = "some kind of magic serialization has resulted in this message";

        private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);

        private class ReaderMock : IClientReader
        {
            private readonly byte[] _message;

            public ReaderMock(byte[] message)
            {
                Guard.DebugAssertArgumentNotNull(message, nameof(message));
                _message = message;
            }

            public int BytesPerCall { get; set; } = int.MaxValue;

            private readonly ManualResetEvent _startEvent = new ManualResetEvent(false);
            public void Start()
            {
                _startEvent.Set();
            }

            private int _position = 0;
            public Task<int> ReceiveDataAsync(byte[] buffer, int offset, int count, CancellationToken ct)
            {
                Guard.DebugAssertArgumentNotNull(buffer, nameof(buffer));
                Guard.DebugAssertArgument(offset >= 0, nameof(offset));
                Guard.DebugAssertArgument(count >= 0, nameof(count));

                _startEvent.WaitOne(_timeout);

                if (_position < _message.Length)
                {
                    var countToCopy = Math.Min(BytesPerCall, count);
                    Array.Copy(_message, _position, buffer, offset, countToCopy);

                    _position += countToCopy;

                    return Task.FromResult(countToCopy);
                }

                return Task.FromResult(0);
            }
        }

        private static IEnumerable<object[]> VariousMessageStrings()
        {
            yield return new object[] { MessageString };
            yield return new object[] { string.Join(Environment.NewLine, Enumerable.Repeat(MessageString, 100500)) };
            yield return new object[] { string.Empty };
        }

        [Theory]
        [MemberData(nameof(VariousMessageStrings))]
        public void TestMessageTransformation(string messageString)
        {
            var sourceMessage = Mock.Of<IClientMessage>();
            var serializedMessage = Encoding.UTF8.GetBytes(messageString);
            var serializerMock = new Mock<IClientMessageSerializer>(MockBehavior.Strict);
            serializerMock.Setup(m => m.Deserialize(serializedMessage)).Returns(sourceMessage);
            var sourceData = BitConverter.GetBytes(serializedMessage.Length).Concat(serializedMessage).ToArray();
            var reader = new ReaderMock(sourceData);
            var messageReceived = new ManualResetEvent(false);
            var requestRounterMock = new Mock<IClientRequestRouter>(MockBehavior.Strict);
            requestRounterMock.Setup(m => m.Process(sourceMessage))
                .Callback(() => messageReceived.Set());

            using (new MessageReaderPipeline(reader, serializerMock.Object, requestRounterMock.Object, CancellationToken.None))
            {
                reader.Start();

                Assert.True(messageReceived.WaitOne(_timeout));
                serializerMock.Verify(m => m.Deserialize(It.IsAny<byte[]>()), Times.AtLeastOnce);
                requestRounterMock.Verify(m => m.Process(sourceMessage), Times.Once);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(sizeof(int))]
        [InlineData(sizeof(int) - 1)]
        [InlineData(sizeof(int) + 1)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        public void TestReceiveMessageByPieces(int bytesPerCall)
        {
            var sourceMessage = Mock.Of<IClientMessage>();
            var serializedMessage = Encoding.UTF8.GetBytes(MessageString);
            var serializerMock = new Mock<IClientMessageSerializer>(MockBehavior.Strict);
            serializerMock.Setup(m => m.Deserialize(serializedMessage)).Returns(sourceMessage);
            var sourceData = BitConverter.GetBytes(serializedMessage.Length).Concat(serializedMessage).ToArray();
            var reader = new ReaderMock(sourceData) {BytesPerCall = bytesPerCall};
            var messageReceived = new ManualResetEvent(false);
            var requestRounterMock = new Mock<IClientRequestRouter>(MockBehavior.Strict);
            requestRounterMock.Setup(m => m.Process(sourceMessage))
                .Callback(() => messageReceived.Set());

            using (new MessageReaderPipeline(reader, serializerMock.Object, requestRounterMock.Object, CancellationToken.None))
            {
                reader.Start();

                Assert.True(messageReceived.WaitOne(_timeout));
                serializerMock.Verify(m => m.Deserialize(It.IsAny<byte[]>()), Times.AtLeastOnce);
                requestRounterMock.Verify(m => m.Process(sourceMessage), Times.Once);
            }
        }
    }
}