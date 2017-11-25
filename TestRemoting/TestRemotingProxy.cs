using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.ServiceFabric.Services.Remoting.V2.Messaging;
using SceneSkope.ServiceFabric.Remoting.Bond;
using Xunit;

namespace TestRemoting
{
    public class TestRemotingProxy
    {
        [Fact]
        public void TestCreatingRequestMessage()
        {
            var provider = new ServiceRemotingBondSerializationProvider();
            var bodyFactory = provider.CreateMessageBodyFactory();
            var requestBody = bodyFactory.CreateRequest(nameof(IDummyRemotingService), nameof(IDummyRemotingService.BasicMethodAsync), 3);

            var segment = new ArraySegment<byte>(Encoding.UTF8.GetBytes("this is a test"));
            requestBody.SetParameter(0, "param1", "machine");
            requestBody.SetParameter(1, "param2", 10);
            requestBody.SetParameter(2, "param3", segment);

            requestBody.GetParameter(0, "param1", typeof(string)).ShouldBeEquivalentTo("machine");
            requestBody.GetParameter(1, "param2", typeof(int)).ShouldBeEquivalentTo(10);
            requestBody.GetParameter(2, "param3", typeof(ArraySegment<byte>)).ShouldBeEquivalentTo(segment);

            var serializer = provider.CreateRequestMessageSerializer(typeof(IDummyRemotingService), new[] { typeof(string), typeof(int), typeof(ArraySegment<byte>) });
            using (var outgoingMessageBody = serializer.Serialize(requestBody))
            using (var stream = new MemoryStream())
            using (var incomingMessageBody = new IncomingMessageBody(stream))
            {
                outgoingMessageBody.Should().NotBeNull();
                foreach (var seg in outgoingMessageBody.GetSendBuffers())
                {
                    stream.Write(seg.Array, seg.Offset, seg.Count);
                }
                stream.Position = 0;
                var receivedBody = serializer.Deserialize(incomingMessageBody);
                receivedBody.GetParameter(0, "param1", typeof(string)).ShouldBeEquivalentTo("machine");
                receivedBody.GetParameter(1, "param2", typeof(int)).ShouldBeEquivalentTo(10);
                receivedBody.GetParameter(2, "param3", typeof(ArraySegment<byte>)).ShouldBeEquivalentTo(segment);
            }
        }
    }
}
