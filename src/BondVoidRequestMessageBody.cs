// --------------------------------------------------------------------------------
// <copyright file="BondVoidRequestMessageBody.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace Microsoft.ServiceFabric.Services.Remoting.V2.Bond
{
    using Microsoft.ServiceFabric.Services.Remoting.V2;
    using System;

    [Bond.Schema]
    internal class BondVoidRequestMessageBody : IServiceRemotingRequestMessageBody
    {
        public object GetParameter(int position, string parameName, Type paramType) => null;

        public void SetParameter(int position, string parameName, object parameter)
        {
            // No-op.
        }
    }
}
