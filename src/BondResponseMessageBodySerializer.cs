// --------------------------------------------------------------------------------
// <copyright file="BondResponseMessageBodySerializer.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace ServiceFabric.Bond.Remoting
{
    using System;
    using global::Bond;
    using global::Bond.IO.Unsafe;
    using global::Bond.Protocols;
    using Microsoft.ServiceFabric.Services.Remoting.V2;
    using Microsoft.ServiceFabric.Services.Remoting.V2.Messaging;

    internal class BondResponseMessageBodySerializer : IServiceRemotingResponseMessageBodySerializer
    {
        private readonly BondGeneratedResponseType generatedResponseType;
        private readonly Serializer<CompactBinaryWriter<OutputBuffer>> serializer;
        private readonly Deserializer<CompactBinaryReader<InputStream>> deserializer;

        public BondResponseMessageBodySerializer(Type responseType)
        {
            if (responseType == null)
            {
                this.generatedResponseType = new BondGeneratedResponseType
                {
                    Type = typeof(BondEmptyResponseMessageBody),
                    InstanceFactory = (_) => new BondEmptyResponseMessageBody(),
                };
            }
            else
            {
                this.generatedResponseType = BondResponseMessageBodyTypeGenerator.Instance.Generate(responseType);
            }

            this.serializer = new Serializer<CompactBinaryWriter<OutputBuffer>>(this.generatedResponseType.Type);
            this.deserializer = new Deserializer<CompactBinaryReader<InputStream>>(this.generatedResponseType.Type);
        }

        public IServiceRemotingResponseMessageBody Deserialize(IIncomingMessageBody messageBody)
        {
            var bondReader = new CompactBinaryReader<InputStream>(new InputStream(messageBody.GetReceivedBuffer(), 1024));
            return this.deserializer.Deserialize(bondReader) as IServiceRemotingResponseMessageBody;
        }

        public IOutgoingMessageBody Serialize(IServiceRemotingResponseMessageBody serviceRemotingResponseMessageBody)
        {
            var generatedResponse = this.generatedResponseType.InstanceFactory(serviceRemotingResponseMessageBody);
            var outputBuffer = new OutputBuffer(1024);
            var bondWriter = new CompactBinaryWriter<OutputBuffer>(outputBuffer);
            this.serializer.Serialize(generatedResponse, bondWriter);
            return new OutgoingMessageBody(new[] { outputBuffer.Data });
        }
    }
}
