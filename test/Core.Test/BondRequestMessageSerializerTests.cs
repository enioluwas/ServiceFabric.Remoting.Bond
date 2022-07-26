using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoFixture;
using AutoFixture.Kernel;
using Xunit;

namespace ServiceFabric.Remoting.Bond.Test
{
    public class BondRequestMessageSerializerTests
    {
        [Fact]
        public void RoundTrip()
        {
            var fixture = new Fixture();
            var context = new SpecimenContext(fixture);
            Type[] types = new[]
            {
                typeof(BondGameInfo),
                typeof(List<BondGameInfo>),
                typeof(HashSet<string>),
                typeof(Dictionary<string, BondGameInfo>),
                typeof(int),
                typeof(string),
                typeof(bool),
                typeof(double),
                typeof(float),
                typeof(BondGameInfo[]),
                typeof(TimeSpan),
            };

            object[] parameters = types.Select((type) => fixture.Create(type, context)).ToArray();

            var serializer = new BondRequestMessageBodySerializer(typeof(BondTypeAliasConverter));
            var requestMessage = new BondRequestMessageBody("TestInterface", "TestMethod", types.Length);
            for (int i = 0; i < parameters.Length; i++)
            {
                requestMessage.SetParameter(i, "", parameters[i]);
            }

            var serializedMessage = serializer.Serialize(requestMessage);

            var buffers = serializedMessage.GetSendBuffers();
            var incomingStream = new MemoryStream(buffers.First().ToArray());
            using var incomingMessage = new MockIncomingMessageBody(incomingStream);
            var receivedRequestMessage = serializer.Deserialize(incomingMessage);
            var receivedParameters = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                receivedParameters[i] = receivedRequestMessage.GetParameter(i, "", typeof(void));
            }

            Assert.Equal<IEnumerable<object>>(parameters, receivedParameters);
        }
    }
}
