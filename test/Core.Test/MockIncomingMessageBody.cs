using System.IO;
using Microsoft.ServiceFabric.Services.Remoting.V2.Messaging;

namespace ServiceFabric.Remoting.Bond.Test
{
    internal class MockIncomingMessageBody : IIncomingMessageBody
    {
        private readonly Stream receivedBuffer;

        public MockIncomingMessageBody(Stream receivedBuffer) => this.receivedBuffer = receivedBuffer;

        public void Dispose() => this.receivedBuffer.Dispose();

        public Stream GetReceivedBuffer() => this.receivedBuffer;
    }
}
