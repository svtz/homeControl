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
    public class MessageWriterPipelineTests
    {
        private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);
        private const string MessageString = "some kind if magic serialization has resulted in this message";

        [Fact]
        public void TestMessageTransformation()
        {
            var message = Mock.Of<IClientMessage>();
            var serializedMessage = Encoding.UTF8.GetBytes(MessageString);
            var serializerMock = new Mock<IClientMessageSerializer>(MockBehavior.Strict);
            serializerMock.Setup(m => m.Serialize(message)).Returns(serializedMessage);
            var expectedData = BitConverter.GetBytes(serializedMessage.Length).Concat(serializedMessage).ToArray();
            var writtenData = new List<byte>(expectedData.Length);
            var writerComplete = new ManualResetEvent(false);
            var writerMock = new Mock<IClientsWriter>(MockBehavior.Strict);
            writerMock.Setup(m => m.WriteAsync(It.IsAny<byte[]>()))
                .Returns((byte[] data) => Task.Factory.StartNew(() =>
                {
                    writtenData.AddRange(data);
                    if (writtenData.Count >= expectedData.Length)
                    {
                        writerComplete.Set();
                    }
                }));

            new MessageWriterPipeline(writerMock.Object, serializerMock.Object).PostMessage(message);
            writerComplete.WaitOne(_timeout);

            serializerMock.Verify(m => m.Serialize(message), Times.AtLeastOnce);
            Assert.True(expectedData.SequenceEqual(writtenData));
        }

        [Fact]
        public void TestLongLongMessageTransformation()
        {
            var message = Mock.Of<IClientMessage>();
            var serializedMessage = Encoding.UTF8.GetBytes(
                string.Join(Environment.NewLine, Enumerable.Repeat(MessageString, 100500)));
            var serializerMock = new Mock<IClientMessageSerializer>(MockBehavior.Strict);
            serializerMock.Setup(m => m.Serialize(message)).Returns(serializedMessage);
            var expectedData = BitConverter.GetBytes(serializedMessage.Length).Concat(serializedMessage).ToArray();
            var writtenData = new List<byte>(expectedData.Length);
            var writerComplete = new ManualResetEvent(false);
            var writerMock = new Mock<IClientsWriter>(MockBehavior.Strict);
            writerMock.Setup(m => m.WriteAsync(It.IsAny<byte[]>()))
                .Returns((byte[] data) => Task.Factory.StartNew(() =>
                {
                    writtenData.AddRange(data);
                    if (writtenData.Count >= expectedData.Length)
                    {
                        writerComplete.Set();
                    }
                }));

            new MessageWriterPipeline(writerMock.Object, serializerMock.Object).PostMessage(message);
            writerComplete.WaitOne(_timeout);

            serializerMock.Verify(m => m.Serialize(message), Times.AtLeastOnce);
            Assert.True(expectedData.SequenceEqual(writtenData));
        }

        [Fact]
        public void TestZeroLengthMessageTransformation()
        {
            var message = Mock.Of<IClientMessage>();
            var serializedMessage = Encoding.UTF8.GetBytes(string.Empty);
            var serializerMock = new Mock<IClientMessageSerializer>(MockBehavior.Strict);
            serializerMock.Setup(m => m.Serialize(message)).Returns(serializedMessage);
            var expectedData = BitConverter.GetBytes(serializedMessage.Length).Concat(serializedMessage).ToArray();
            var writtenData = new List<byte>(expectedData.Length);
            var writerComplete = new ManualResetEvent(false);
            var writerMock = new Mock<IClientsWriter>(MockBehavior.Strict);
            writerMock.Setup(m => m.WriteAsync(It.IsAny<byte[]>()))
                .Returns((byte[] data) => Task.Factory.StartNew(() =>
                {
                    writtenData.AddRange(data);
                    if (writtenData.Count >= expectedData.Length)
                    {
                        writerComplete.Set();
                    }
                }));

            new MessageWriterPipeline(writerMock.Object, serializerMock.Object).PostMessage(message);
            writerComplete.WaitOne(_timeout);

            serializerMock.Verify(m => m.Serialize(message), Times.AtLeastOnce);
            Assert.True(expectedData.SequenceEqual(writtenData));
        }

    }
}
