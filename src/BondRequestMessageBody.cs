// --------------------------------------------------------------------------------
// <copyright file="BondRequestMessageBody.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace Microsoft.ServiceFabric.Services.Remoting.V2.Bond
{
    using Microsoft.ServiceFabric.Services.Remoting.V2;
    using System;

    internal class BondRequestMessageBody : IServiceRemotingRequestMessageBody
    {
        private readonly object[] parameters;

        public BondRequestMessageBody(int numParameters)
        {
            this.parameters = new object[numParameters];
        }

        public object GetParameter(int position, string parameName, Type paramType) => this.parameters[position];

        public void SetParameter(int position, string parameName, object parameter) => this.parameters[position] = parameter;
    }
}
