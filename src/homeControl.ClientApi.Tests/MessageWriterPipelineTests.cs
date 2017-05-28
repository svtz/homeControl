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

        private static IEnumerable<object[]> VariousMessageStrings()
        {
            const string messageString = "some kind of magic serialization has resulted in this message";

            yield return new object[] { messageString };
            yield return new object[] { string.Join(Environment.NewLine, Enumerable.Repeat(messageString, 100500)) };
            yield return new object[] { string.Empty };
        }

        [Theory]
        [MemberData(nameof(VariousMessageStrings))]
        public void TestMessageTransformation(string messageString)
        {
            var message = Mock.Of<IClientMessage>();
            var serializedMessage = Encoding.UTF8.GetBytes(messageString);
            var serializerMock = new Mock<IClientMessageSerializer>(MockBehavior.Strict);
            serializerMock.Setup(m => m.Serialize(message)).Returns(serializedMessage);
            var expectedData = BitConverter.GetBytes(serializedMessage.Length).Concat(serializedMessage).ToArray();
            var writtenData = new List<byte>(expectedData.Length);
            var writerComplete = new ManualResetEvent(false);
            var writerMock = new Mock<IClientWriter>(MockBehavior.Strict);
            writerMock.Setup(m => m.WriteAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
                .Returns((byte[] data, CancellationToken ct) => Task.Factory.StartNew(() =>
                {
                    writtenData.AddRange(data);
                    if (writtenData.Count >= expectedData.Length)
                    {
                        writerComplete.Set();
                    }
                }, ct));

            using (var writer = new MessageWriterPipeline(writerMock.Object, serializerMock.Object, CancellationToken.None))
            {
                writer.PostMessage(message);

                Assert.True(writerComplete.WaitOne(_timeout));
                Assert.True(expectedData.SequenceEqual(writtenData));
                serializerMock.Verify(m => m.Serialize(message), Times.AtLeastOnce);
            }
        }
    }
}
