// --------------------------------------------------------------------------------
// <copyright file="BondRequestMessageBody.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace ServiceFabric.Remoting.Bond
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.ServiceFabric.Services.Remoting.V2;

    internal class BondRequestMessageBody : IServiceRemotingRequestMessageBody
    {
        private readonly object[] parameters;
        public string InterfaceName { get; }
        public string MethodName { get; }
        public List<string> ParameterTypeNames { get; }

        public BondRequestMessageBody(string interfaceName, string methodName, int numParameters)
        {
            this.InterfaceName = interfaceName;
            this.MethodName = methodName;
            this.parameters = new object[numParameters];
            this.ParameterTypeNames = new List<string>(Enumerable.Repeat(string.Empty, numParameters));
        }

        public object GetParameter(int position, string parameName, Type paramType) => this.parameters[position];

        public void SetParameter(int position, string parameName, object parameter)
        {
            this.parameters[position] = parameter;
            this.ParameterTypeNames[position] = parameter.GetType().AssemblyQualifiedName;
        }
    }
}
