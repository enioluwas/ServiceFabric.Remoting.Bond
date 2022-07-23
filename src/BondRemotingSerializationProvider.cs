// --------------------------------------------------------------------------------
// <copyright file="BondRemotingSerializationProvider.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace ServiceFabric.Remoting.Bond
{
    using System;
    using System.Collections.Generic;
    using Microsoft.ServiceFabric.Services.Remoting.V2;

    public class BondRemotingSerializationProvider : IServiceRemotingMessageSerializationProvider
    {
        public IServiceRemotingMessageBodyFactory CreateMessageBodyFactory()
        {
            return new BondMessageFactory();
        }

        public IServiceRemotingRequestMessageBodySerializer CreateRequestMessageSerializer(Type serviceInterfaceType, IEnumerable<Type> methodParameterTypes, IEnumerable<Type> wrappedMessageTypes = null)
        {
            return new BondRequestMessageBodySerializer();
        }

        public IServiceRemotingResponseMessageBodySerializer CreateResponseMessageSerializer(Type serviceInterfaceType, IEnumerable<Type> methodParameterTypes, IEnumerable<Type> wrappedMessageTypes = null)
        {
            return new BondResponseMessageBodySerializer();
        }
    }
}
