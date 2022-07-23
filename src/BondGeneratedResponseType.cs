// --------------------------------------------------------------------------------
// <copyright file="BondGeneratedResponseType.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace ServiceFabric.Remoting.Bond
{
    using global::Bond;
    using global::Bond.IO.Unsafe;
    using global::Bond.Protocols;
    using Microsoft.ServiceFabric.Services.Remoting.V2;
    using System;

    internal class BondGeneratedResponseType
    {
        public Type Type { get; init; }
        public Func<IServiceRemotingResponseMessageBody, object> InstanceFactory { get; init; }
        public Deserializer<CompactBinaryReader<InputBuffer>> Deserializer { get; init; }
        public Serializer<CompactBinaryWriter<OutputBuffer>> Serializer { get; init; }
    }
}
