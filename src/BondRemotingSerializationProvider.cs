// --------------------------------------------------------------------------------
// <copyright file="BondRemotingSerializationProvider.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace ServiceFabric.Remoting.Bond
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.ServiceFabric.Services.Remoting.V2;

    public class BondRemotingSerializationProvider : IServiceRemotingMessageSerializationProvider
    {
        private static readonly ConcurrentDictionary<List<Type>, BondRequestMessageBodySerializer> RequestSerializerCache = new ConcurrentDictionary<List<Type>, BondRequestMessageBodySerializer>(new TypeListEqualityComparer());
        private static readonly ConcurrentDictionary<Type, BondResponseMessageBodySerializer> ResponseSerializerCache = new ConcurrentDictionary<Type, BondResponseMessageBodySerializer>();
        private static BondRequestMessageBodySerializer EmptyRequestSerializer;
        private static BondResponseMessageBodySerializer EmptyResponseSerializer;

        public IServiceRemotingMessageBodyFactory CreateMessageBodyFactory()
        {
            return new BondMessageFactory();
        }

        public IServiceRemotingRequestMessageBodySerializer CreateRequestMessageSerializer(Type serviceInterfaceType, IEnumerable<Type> methodParameterTypes, IEnumerable<Type> wrappedMessageTypes = null)
        {
            if (!methodParameterTypes.Any())
            {
                return EmptyRequestSerializer ??= new BondRequestMessageBodySerializer(Enumerable.Empty<Type>());
            }

            return RequestSerializerCache.GetOrAdd(methodParameterTypes.ToOrAsList(), (types) => new BondRequestMessageBodySerializer(types));
        }

        public IServiceRemotingResponseMessageBodySerializer CreateResponseMessageSerializer(Type serviceInterfaceType, IEnumerable<Type> methodParameterTypes, IEnumerable<Type> wrappedMessageTypes = null)
        {
            var responseType = methodParameterTypes.FirstOrDefault();
            if (responseType == null)
            {
                return EmptyResponseSerializer ??= new BondResponseMessageBodySerializer(null);
            }

            return ResponseSerializerCache.GetOrAdd(methodParameterTypes.First(), (type) => new BondResponseMessageBodySerializer(type));
        }
    }
}
