// --------------------------------------------------------------------------------
// <copyright file="BondResponseMessageBody.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace Microsoft.ServiceFabric.Services.Remoting.V2.Bond
{
    using Microsoft.ServiceFabric.Services.Remoting.V2;
    using System;

    internal class BondResponseMessageBody : IServiceRemotingResponseMessageBody
    {
        private object response;

        public object Get(Type paramType) => this.response;

        public void Set(object response) => this.response = response;
    }
}
