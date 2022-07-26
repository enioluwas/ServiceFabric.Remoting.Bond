using System.IO;
using System.Linq;
using AutoFixture;
using Xunit;

namespace ServiceFabric.Remoting.Bond.Test
{
    public class BondResponseMessageSerializerTests
    {
        [Fact]
        public void RoundTrip()
        {
            var serializer = new BondResponseMessageBodySerializer(typeof(BondTypeAliasConverter));
            var response = new Fixture().Create<BondGameInfo>();
            var responseMessage = new BondResponseMessageBody("TestInterface", "TestMethod");
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
