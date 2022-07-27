// --------------------------------------------------------------------------------
// <copyright file="BondResponseMessageBodySerializer.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace ServiceFabric.Remoting.Bond
{
    using System;
    using System.Collections.Concurrent;
    using global::Bond;
    using global::Bond.IO.Unsafe;
    using global::Bond.Protocols;
    using Microsoft.ServiceFabric.Services.Remoting.V2;
    using Microsoft.ServiceFabric.Services.Remoting.V2.Messaging;

    internal class BondResponseMessageBodySerializer : IServiceRemotingResponseMessageBodySerializer
    {
        private readonly BondResponseMessageBodyTypeGenerator typeGenerator;
        private readonly Serializer<FastBinaryWriter<OutputBuffer>> serializer;
        private readonly Deserializer<FastBinaryReader<InputStream>> deserializer;
        private readonly ConcurrentDictionary<ResponseCacheKey, BondGeneratedResponseType> generatedTypeCache;

        public BondResponseMessageBodySerializer(Type converterType)
        {
            this.typeGenerator = new BondResponseMessageBodyTypeGenerator(converterType);
            this.serializer = new Serializer<FastBinaryWriter<OutputBuffer>>(typeof(BondResponseMessageMetadata));
            this.deserializer = new Deserializer<FastBinaryReader<InputStream>>(typeof(BondResponseMessageMetadata));
            this.generatedTypeCache = new ConcurrentDictionary<ResponseCacheKey, BondGeneratedResponseType>();
        }

        public IServiceRemotingResponseMessageBody Deserialize(IIncomingMessageBody messageBody)
        {
            var wrappedReader = new FastBinaryReader<InputStream>(new InputStream(messageBody.GetReceivedBuffer(), 1024));
            var wrappedMessage = this.deserializer.Deserialize<BondResponseMessageMetadata>(wrappedReader);

            if (wrappedMessage == null || wrappedMessage.ReturnTypeName == Constants.VoidTypeName)
            {
                return new BondEmptyResponseMessageBody();
            }

            var cacheKey = new ResponseCacheKey
            {
                MethodName = wrappedMessage.MethodName,
                ReturnTypeName = wrappedMessage.ReturnTypeName,
            };

            var generatedType = this.GetOrAddGeneratedType(cacheKey);
            var innerReader = new FastBinaryReader<InputBuffer>(new InputBuffer(wrappedMessage.Payload));
            return generatedType.Deserializer.Deserialize<IServiceRemotingResponseMessageBody>(innerReader);
        }

        public IOutgoingMessageBody Serialize(IServiceRemotingResponseMessageBody serviceRemotingResponseMessageBody)
        {
            if (serviceRemotingResponseMessageBody == null)
            {
                return null;
            }

            var bondMessageBody = (BondResponseMessageBody)serviceRemotingResponseMessageBody;

            if (bondMessageBody.ReturnTypeName == Constants.VoidTypeName)
            {
                var message = new BondResponseMessageMetadata
                {
                    MethodName = bondMessageBody.MethodName,
                    ReturnTypeName = bondMessageBody.ReturnTypeName,
                };

                var buffer = new OutputBuffer(256);
                var writer = new FastBinaryWriter<OutputBuffer>(buffer);
                this.serializer.Serialize(message, writer);
                return new OutgoingMessageBody(new[] { buffer.Data });
            }

            var cacheKey = new ResponseCacheKey
            {
                MethodName = bondMessageBody.MethodName,
                ReturnTypeName = bondMessageBody.ReturnTypeName,
            };

            var generatedType = this.GetOrAddGeneratedType(cacheKey);
            var innerMessage = generatedType.InstanceFactory(bondMessageBody);
            var innerBuffer = new OutputBuffer(1024);
            var innerMessageWriter = new FastBinaryWriter<OutputBuffer>(innerBuffer);
            generatedType.Serializer.Serialize(innerMessage, innerMessageWriter);

            var wrappedMessage = new BondResponseMessageMetadata
            {
                MethodName = bondMessageBody.MethodName,
                ReturnTypeName = bondMessageBody.ReturnTypeName,
                Payload = innerBuffer.Data,
            };

            var wrappedBuffer = new OutputBuffer(innerBuffer.Data.Count + 1024);
            var wrappedWriter = new FastBinaryWriter<OutputBuffer>(wrappedBuffer);
            this.serializer.Serialize(wrappedMessage, wrappedWriter);
            return new OutgoingMessageBody(new[] { wrappedBuffer.Data });
        }

        private BondGeneratedResponseType GetOrAddGeneratedType(ResponseCacheKey cacheKey)
        {
            return this.generatedTypeCache.GetOrAdd(
                cacheKey,
                (key) => this.typeGenerator.Generate(Type.GetType(key.ReturnTypeName)));
        }

        private readonly struct ResponseCacheKey
        {
            public string MethodName { get; init; }
            public string ReturnTypeName { get; init; }

            public override bool Equals(object obj)
            {
                if (obj is ResponseCacheKey other)
                {
                    return this.Equals(other);
                }

                return false;
            }

            public bool Equals(ResponseCacheKey other)
            {
                return this.MethodName == other.MethodName
                    && this.ReturnTypeName == other.ReturnTypeName;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(this.MethodName, this.ReturnTypeName);
            }
        }
    }
}
