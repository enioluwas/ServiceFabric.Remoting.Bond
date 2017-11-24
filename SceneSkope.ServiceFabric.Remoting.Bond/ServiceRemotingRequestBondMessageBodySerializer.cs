using System;
using System.Collections.Generic;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Messaging;
using Bond;
using Bond.Protocols;
using Bond.IO.Unsafe;
using System.Linq;
using System.IO;
using Sigil;
using System.Reflection.Emit;

namespace SceneSkope.ServiceFabric.Remoting.Bond
{
    internal class ServiceRemotingRequestBondMessageBodySerializer : IServiceRemotingRequestMessageBodySerializer
    {
        static ServiceRemotingRequestBondMessageBodySerializer()
        {
            var assembly = typeof(IBufferPoolManager).Assembly;
            var type = assembly.GetType("Microsoft.ServiceFabric.Services.Remoting.V2.Messaging.SegmentedPoolMemoryStream");
            var constructor = type.GetConstructor(new[] { typeof(IBufferPoolManager) });
            var ce = Emit<Func<IBufferPoolManager, Stream>>.NewDynamicMethod();
            ce.LoadArgument(0);
            ce.NewObject(constructor);
            ce.Return();
            _createSegmentedPoolMemoryStream = ce.CreateDelegate();

            var getBuffersMethod = type.GetMethod("GetBuffers");
            var gbe = Emit<Func<Stream, IEnumerable<IPooledBuffer>>>.NewDynamicMethod();
            gbe.LoadArgument(0);
            gbe.CastClass(type);
            gbe.CallVirtual(getBuffersMethod);
            gbe.Return();
            _getBuffers = gbe.CreateDelegate();
        }

        private static readonly Func<IBufferPoolManager, Stream> _createSegmentedPoolMemoryStream;
        private static readonly Func<Stream, IEnumerable<IPooledBuffer>> _getBuffers;

        private readonly int _parameterCount;
        private readonly Type _payloadType;
        private readonly Serializer<FastBinaryWriter<OutputStream>> _serializer;
        private readonly Deserializer<FastBinaryReader<InputStream>> _deserializer;
        private readonly IBufferPoolManager _bufferPoolManager;

        public ServiceRemotingRequestBondMessageBodySerializer(IBufferPoolManager bufferPoolManager, IList<Type> requestBodyTypes)
        {
            _bufferPoolManager = bufferPoolManager;
            _parameterCount = requestBodyTypes.Count;
            _payloadType = CachedBondMessageTypeBuilder.GetOrAddRequestType(requestBodyTypes);
            _serializer = new Serializer<FastBinaryWriter<OutputStream>>(_payloadType);
            _deserializer = new Deserializer<FastBinaryReader<InputStream>>(_payloadType);
        }

        public IServiceRemotingRequestMessageBody Deserialize(IncomingMessageBody messageBody)
        {
            using (var stream = messageBody.GetReceivedBuffer())
            {
                var reader = new FastBinaryReader<InputStream>(new InputStream(stream));
                return _deserializer.Deserialize(reader) as IServiceRemotingRequestMessageBody;
            }
        }

        public OutgoingMessageBody Serialize(IServiceRemotingRequestMessageBody serviceRemotingRequestMessageBody)
        {
            var parameters = new object[_parameterCount];
            for (var i = 0; i < _parameterCount; i++)
            {
                parameters[i] = serviceRemotingRequestMessageBody.GetParameter(i, "", typeof(void));
            }
            var constructed = _payloadType.GetConstructors()[1].Invoke(parameters);
            using (var stream = _createSegmentedPoolMemoryStream(_bufferPoolManager))
            {
                var outputStream = new OutputStream(stream);
                var writer = new FastBinaryWriter<OutputStream>(outputStream);
                _serializer.Serialize(constructed, writer);
                outputStream.Flush();
                var buffers = _getBuffers(stream);
                return new OutgoingMessageBody(buffers);
            }
        }
    }
}