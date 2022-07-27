// --------------------------------------------------------------------------------
// <copyright file="BondGeneratedRequestType.cs" company="Microsoft Corporation">
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

    internal class BondGeneratedRequestType
    {
        public Type Type { get; init; }
        public Func<IServiceRemotingRequestMessageBody, object> InstanceFactory { get; init; }
        public Deserializer<FastBinaryReader<InputBuffer>> Deserializer { get; init; }
        public Serializer<FastBinaryWriter<OutputBuffer>> Serializer { get; init; }
    }
}
