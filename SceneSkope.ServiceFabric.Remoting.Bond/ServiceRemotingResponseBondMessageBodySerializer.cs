using Bond;
using Bond.IO.Unsafe;
using Bond.Protocols;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SceneSkope.ServiceFabric.Remoting.Bond
{
    internal class ServiceRemotingResponseBondMessageBodySerializer : IServiceRemotingResponseMessageBodySerializer
    {
        private readonly bool _empty;
        private readonly Type _payloadType;
        private readonly Serializer<FastBinaryWriter<OutputStream>> _serializer;
        private readonly Deserializer<FastBinaryReader<InputStream>> _deserializer;

        public ServiceRemotingResponseBondMessageBodySerializer(IEnumerable<Type> responseBodyTypes)
        {
            var responseBodyType = responseBodyTypes.SingleOrDefault();
            _empty = responseBodyType == null;
            if (_empty)
            {
                _payloadType = typeof(BondEmptyResponseMessageBody);
            }
            else
            {
                _payloadType = CachedBondMessageTypeBuilder.GetOrAddResponseType(responseBodyType);
            }
            _serializer = new Serializer<FastBinaryWriter<OutputStream>>(_payloadType);
            _deserializer = new Deserializer<FastBinaryReader<InputStream>>(_payloadType);
        }

        public IServiceRemotingResponseMessageBody Deserialize(IncomingMessageBody messageBody)
        {
            using (var stream = messageBody.GetReceivedBuffer())
            {
                var reader = new FastBinaryReader<InputStream>(new InputStream(stream));
                return _deserializer.Deserialize(reader) as IServiceRemotingResponseMessageBody;
            }
        }

        public OutgoingMessageBody Serialize(IServiceRemotingResponseMessageBody serviceRemotingRequestMessageBody)
        {
            object constructed;
            if (_empty)
            {
                constructed = new BondEmptyResponseMessageBody();
            }
            else
            {
                var value = serviceRemotingRequestMessageBody.Get(typeof(object));
                constructed = _payloadType.GetConstructors()[1].Invoke(new[] { value });
            }

            using (var stream = new MemoryStream())
            {
                var outputStream = new OutputStream(stream);
                var writer = new FastBinaryWriter<OutputStream>(outputStream);
                _serializer.Serialize(constructed, writer);
                outputStream.Flush();
                stream.TryGetBuffer(out var segment);
                return new OutgoingMessageBody(new[] { segment });
            }
        }
    }
}