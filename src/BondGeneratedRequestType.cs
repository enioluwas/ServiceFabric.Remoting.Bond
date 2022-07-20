// --------------------------------------------------------------------------------
// <copyright file="BondGeneratedRequestType.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace ServiceFabric.Bond.Remoting
{
    using Microsoft.ServiceFabric.Services.Remoting.V2;
    using System;

    internal readonly struct BondGeneratedRequestType
    {
        public Type Type { get; init; }
        public Func<IServiceRemotingRequestMessageBody, object> InstanceFactory { get; init; }
    }
}
