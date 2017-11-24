using Microsoft.ServiceFabric.Services.Remoting.V2;
using System;

namespace SceneSkope.ServiceFabric.Remoting.Bond
{
    internal class BondMessageFactory : IServiceRemotingMessageBodyFactory
    {
        public IServiceRemotingRequestMessageBody CreateRequest(string interfaceName, string methodName, int numberOfParameters)
        {
            return new BondRequestMessageBody(numberOfParameters);
        }

        public IServiceRemotingResponseMessageBody CreateResponse(string interfaceName, string methodName) =>
            new BondResponseMessageBody();
    }
}