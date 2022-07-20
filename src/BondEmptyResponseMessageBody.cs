// --------------------------------------------------------------------------------
// <copyright file="BondEmptyResponseMessageBody.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace ServiceFabric.Remoting.Bond
{
    using Microsoft.ServiceFabric.Services.Remoting.V2;
    using System;

    [global::Bond.Schema]
    internal class BondEmptyResponseMessageBody : IServiceRemotingResponseMessageBody
    {
        public object Get(Type paramType) => null;

        public void Set(object response)
        {
            // No-op.
        }
    }
}
