// --------------------------------------------------------------------------------
// <copyright file="BondRequestMessageBodySerializer.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace ServiceFabric.Remoting.Bond
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using global::Bond;
    using global::Bond.IO.Unsafe;
    using global::Bond.Protocols;
    using Microsoft.ServiceFabric.Services.Remoting.V2;
    using Microsoft.ServiceFabric.Services.Remoting.V2.Messaging;

    internal class BondRequestMessageBodySerializer : IServiceRemotingRequestMessageBodySerializer
    {
        private readonly BondRequestMessageBodyTypeGenerator typeGenerator;
        private readonly Serializer<CompactBinaryWriter<OutputBuffer>> serializer;
        private readonly Deserializer<CompactBinaryReader<InputStream>> deserializer;
        private readonly ConcurrentDictionary<RequestCacheKey, BondGeneratedRequestType> generatedTypeCache;

        public BondRequestMessageBodySerializer(Type converterType)
        {
            this.typeGenerator = new BondRequestMessageBodyTypeGenerator(converterType);
            this.serializer = new Serializer<CompactBinaryWriter<OutputBuffer>>(typeof(BondRequestMessageMetadata));
            this.deserializer = new Deserializer<CompactBinaryReader<InputStream>>(typeof(BondRequestMessageMetadata));
            this.generatedTypeCache = new ConcurrentDictionary<RequestCacheKey, BondGeneratedRequestType>();
        }

        public IServiceRemotingRequestMessageBody Deserialize(IIncomingMessageBody messageBody)
        {
            var wrappedReader = new CompactBinaryReader<InputStream>(new InputStream(messageBody.GetReceivedBuffer(), 1024));
            var wrappedMessage = this.deserializer.Deserialize<BondRequestMessageMetadata>(wrappedReader);

            if (wrappedMessage.ParameterTypeNames.Count == 0)
            {
                return new BondEmptyRequestMessageBody();
            }

            var cacheKey = new RequestCacheKey
            {
                MethodName = wrappedMessage.MethodName,
                ParameterTypeNames = wrappedMessage.ParameterTypeNames,
            };

            var generatedType = this.GetOrAddGeneratedType(cacheKey);
            var innerReader = new CompactBinaryReader<InputBuffer>(new InputBuffer(wrappedMessage.Payload));
            return generatedType.Deserializer.Deserialize<IServiceRemotingRequestMessageBody>(innerReader);
        }

        public IOutgoingMessageBody Serialize(IServiceRemotingRequestMessageBody serviceRemotingRequestMessageBody)
        {
            var bondMessageBody = (BondRequestMessageBody)serviceRemotingRequestMessageBody;

            if (bondMessageBody.ParameterTypeNames.Count == 0)
            {
                var message = new BondRequestMessageMetadata
                {
                    MethodName = bondMessageBody.MethodName,
                    ParameterTypeNames = bondMessageBody.ParameterTypeNames,
                };

                var buffer = new OutputBuffer(256);
                var writer = new CompactBinaryWriter<OutputBuffer>(buffer);
                this.serializer.Serialize(message, writer);
                return new OutgoingMessageBody(new[] { buffer.Data });
            }

            var cacheKey = new RequestCacheKey
            {
                MethodName = bondMessageBody.MethodName,
                ParameterTypeNames = bondMessageBody.ParameterTypeNames,
            };

            var generatedType = this.GetOrAddGeneratedType(cacheKey);
            var innerMessage = generatedType.InstanceFactory(bondMessageBody);
            var innerBuffer = new OutputBuffer(1024);
            var innerMessageWriter = new CompactBinaryWriter<OutputBuffer>(innerBuffer);
            generatedType.Serializer.Serialize(innerMessage, innerMessageWriter);

            var wrappedMessage = new BondRequestMessageMetadata
            {
                MethodName = bondMessageBody.MethodName,
                ParameterTypeNames = bondMessageBody.ParameterTypeNames,
                Payload = innerBuffer.Data,
            };

            var wrappedBuffer = new OutputBuffer(innerBuffer.Data.Count + 1024);
            var wrappedWriter = new CompactBinaryWriter<OutputBuffer>(wrappedBuffer);
            this.serializer.Serialize(wrappedMessage, wrappedWriter);
            return new OutgoingMessageBody(new[] { wrappedBuffer.Data });
        }

        private BondGeneratedRequestType GetOrAddGeneratedType(RequestCacheKey cacheKey)
        {
            return this.generatedTypeCache.GetOrAdd(
                cacheKey,
                (key) => this.typeGenerator.Generate(
                    key.ParameterTypeNames.Select((typeName) => Type.GetType(typeName)!).ToArray()));
        }

        private readonly struct RequestCacheKey
        {
            public string MethodName { get; init; }
            public List<string> ParameterTypeNames { get; init; }

            public override bool Equals(object obj)
            {
                if (obj is RequestCacheKey other)
                {
                    return this.Equals(other);
                }

                return false;
            }

            public bool Equals(RequestCacheKey other)
            {
                return this.MethodName == other.MethodName
                    && this.ParameterTypeNames.SequenceEqual(other.ParameterTypeNames);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(this.MethodName, this.ParameterTypeNames);
            }
        }
    }
}
