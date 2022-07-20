// --------------------------------------------------------------------------------
// <copyright file="BondGeneratedResponseType.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace ServiceFabric.Remoting.Bond
{
    using Microsoft.ServiceFabric.Services.Remoting.V2;
    using System;

    internal readonly struct BondGeneratedResponseType
    {
        public Type Type { get; init; }
        public Func<IServiceRemotingResponseMessageBody, object> InstanceFactory { get; init; }
    }
}
