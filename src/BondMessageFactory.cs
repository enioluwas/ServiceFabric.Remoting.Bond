// --------------------------------------------------------------------------------
// <copyright file="BondMessageFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace ServiceFabric.Remoting.Bond
{
    using Microsoft.ServiceFabric.Services.Remoting.V2;

    internal class BondMessageFactory : IServiceRemotingMessageBodyFactory
    {
        public IServiceRemotingRequestMessageBody CreateRequest(string interfaceName, string methodName, int numberOfParameters, object wrappedRequestObject)
        {
            return new BondRequestMessageBody(interfaceName, methodName, numberOfParameters);
        }

        public IServiceRemotingResponseMessageBody CreateResponse(string interfaceName, string methodName, object wrappedResponseObject)
        {
            return new BondResponseMessageBody(interfaceName, methodName);
        }
    }
}
