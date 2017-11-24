using System;
using Microsoft.ServiceFabric.Services.Remoting.V2;

namespace SceneSkope.ServiceFabric.Remoting.Bond
{
    internal class BondRequestMessageBody : IServiceRemotingRequestMessageBody
    {
        private readonly object[] _parameters;

        public BondRequestMessageBody(int numberOfParameters)
        {
            _parameters = new object[numberOfParameters];
        }

        public object GetParameter(int position, string parameName, Type paramType) => _parameters[position];

        public void SetParameter(int position, string parameName, object parameter) => _parameters[position] = parameter;
    }
}