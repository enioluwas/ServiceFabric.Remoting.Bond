using System;
using Microsoft.ServiceFabric.Services.Remoting.V2;

namespace SceneSkope.ServiceFabric.Remoting.Bond
{
    internal class BondResponseMessageBody : IServiceRemotingResponseMessageBody
    {
        private object _value;
        public object Get(Type paramType) => _value;

        public void Set(object response) => _value = response;
    }
}