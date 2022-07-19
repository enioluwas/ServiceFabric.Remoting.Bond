// --------------------------------------------------------------------------------
// <copyright file="BondVoidResponseMessageBody.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace Microsoft.ServiceFabric.Services.Remoting.V2.Bond
{
    using Microsoft.ServiceFabric.Services.Remoting.V2;
    using System;

    [global::Bond.Schema]
    internal class BondVoidResponseMessageBody : IServiceRemotingResponseMessageBody
    {
        public object Get(Type paramType) => null;

        public void Set(object response)
        {
            // No-op.
        }
    }
}
