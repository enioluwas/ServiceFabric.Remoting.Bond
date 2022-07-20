// --------------------------------------------------------------------------------
// <copyright file="BondEmptyRequestMessageBody.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace ServiceFabric.Remoting.Bond
{
    using Microsoft.ServiceFabric.Services.Remoting.V2;
    using System;

    [global::Bond.Schema]
    internal class BondEmptyRequestMessageBody : IServiceRemotingRequestMessageBody
    {
        public object GetParameter(int position, string parameName, Type paramType) => null;

        public void SetParameter(int position, string parameName, object parameter)
        {
            // No-op.
        }
    }
}
