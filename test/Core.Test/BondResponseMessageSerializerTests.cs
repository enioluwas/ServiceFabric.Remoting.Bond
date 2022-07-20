using System.IO;
using System.Linq;
using AutoFixture;
using ServiceFabric.Bond.Remoting;
using Xunit;

namespace ServiceFabric.Bond.Remoting.Test
{
    public class BondResponseMessageSerializerTests
    {
        [Fact]
        public void RoundTrip()
        {
            var serializer = new BondResponseMessageBodySerializer(typeof(BondGameInfo));
            var response = new Fixture().Create<BondGameInfo>();
            var responseMessage = new BondResponseMessageBody();
            responseMessage.Set(response);
            var serializedMessage = serializer.Serialize(responseMessage);

            var buffers = serializedMessage.GetSendBuffers();
            var incomingStream = new MemoryStream(buffers.First().ToArray());
            using var incomingMessage = new MockIncomingMessageBody(incomingStream);
            var receivedResponseMessage = serializer.Deserialize(incomingMessage);
            var receivedResponse = receivedResponseMessage.Get(typeof(void)) as BondGameInfo;
            Assert.Equal(response, receivedResponse);
        }
    }
}
