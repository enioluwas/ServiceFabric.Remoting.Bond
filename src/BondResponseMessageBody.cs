// --------------------------------------------------------------------------------
// <copyright file="BondResponseMessageBody.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace ServiceFabric.Remoting.Bond
{
    using System;
    using Microsoft.ServiceFabric.Services.Remoting.V2;

    internal class BondResponseMessageBody : IServiceRemotingResponseMessageBody
    {
        private object response;
        public string InterfaceName { get; }
        public string MethodName { get; }
        public string ReturnTypeName { get; private set; }

        public BondResponseMessageBody(string interfaceName, string methodName)
        {
            this.InterfaceName = interfaceName;
            this.MethodName = methodName;
        }

        public object Get(Type paramType) => this.response;

        public void Set(object response)
        {
            this.response = response;
            this.ReturnTypeName = response.GetType().AssemblyQualifiedName;
        }
    }
}
