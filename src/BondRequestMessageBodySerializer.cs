// --------------------------------------------------------------------------------
// <copyright file="BondRequestMessageBodySerializer.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace Microsoft.ServiceFabric.Services.Remoting.V2.Bond
{
    using Bond;
    using global::Bond;
    using global::Bond.IO.Unsafe;
    using global::Bond.Protocols;
    using Microsoft.ServiceFabric.Services.Remoting.V2;
    using Microsoft.ServiceFabric.Services.Remoting.V2.Messaging;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    internal class BondRequestMessageBodySerializer : IServiceRemotingRequestMessageBodySerializer
    {
        private static readonly ConcurrentDictionary<Type[], BondGeneratedRequestType> GeneratedRequestTypeCache = new ConcurrentDictionary<Type[], BondGeneratedRequestType>();
        private readonly BondGeneratedRequestType generatedRequestType;
        private readonly Serializer<CompactBinaryWriter<OutputBuffer>> serializer;
        private readonly Deserializer<CompactBinaryReader<InputStream>> deserializer;

        public BondRequestMessageBodySerializer(IEnumerable<Type> requestBodyTypes)
        {
            if (!requestBodyTypes.Any())
            {
                this.generatedRequestType = new BondGeneratedRequestType
                {
                    Type = typeof(BondVoidRequestMessageBody),
                    InstanceFactory = (_) => new BondVoidRequestMessageBody(),
                };
            }
            else
            {
                this.generatedRequestType = GeneratedRequestTypeCache
                    .GetOrAdd(
                    requestBodyTypes.ToArray(),
                    (types) => BondRequestMessageBodyTypeGenerator.Instance.Generate(types));
            }

            this.serializer = new Serializer<CompactBinaryWriter<OutputBuffer>>(this.generatedRequestType.Type);
            this.deserializer = new Deserializer<CompactBinaryReader<InputStream>>(this.generatedRequestType.Type);
        }

        public IServiceRemotingRequestMessageBody Deserialize(IIncomingMessageBody messageBody)
        {
            var bondReader = new CompactBinaryReader<InputStream>(new InputStream(messageBody.GetReceivedBuffer(), 1024));
            return this.deserializer.Deserialize(bondReader) as IServiceRemotingRequestMessageBody;
        }

        public IOutgoingMessageBody Serialize(IServiceRemotingRequestMessageBody serviceRemotingRequestMessageBody)
        {
            var generatedRequest = this.generatedRequestType.InstanceFactory(serviceRemotingRequestMessageBody);
            var outputBuffer = new OutputBuffer(1024);
            var bondWriter = new CompactBinaryWriter<OutputBuffer>(outputBuffer);
            this.serializer.Serialize(generatedRequest, bondWriter);
            return new OutgoingMessageBody(new[] { outputBuffer.Data });
        }
    }
}
