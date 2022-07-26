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
        private readonly Type converterType;

        public BondRemotingSerializationProvider(Type converterType)
        {
            this.converterType = converterType;
        }

        public BondRemotingSerializationProvider()
        {
        }

        public IServiceRemotingMessageBodyFactory CreateMessageBodyFactory()
        {
            return new BondMessageFactory();
        }

        public IServiceRemotingRequestMessageBodySerializer CreateRequestMessageSerializer(Type serviceInterfaceType, IEnumerable<Type> methodParameterTypes, IEnumerable<Type> wrappedMessageTypes = null)
        {
            return new BondRequestMessageBodySerializer(this.converterType);
        }

        public IServiceRemotingResponseMessageBodySerializer CreateResponseMessageSerializer(Type serviceInterfaceType, IEnumerable<Type> methodParameterTypes, IEnumerable<Type> wrappedMessageTypes = null)
        {
            return new BondResponseMessageBodySerializer(this.converterType);
        }
    }
}
