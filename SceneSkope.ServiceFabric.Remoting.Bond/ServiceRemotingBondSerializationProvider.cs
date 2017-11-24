using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneSkope.ServiceFabric.Remoting.Bond
{
    public class ServiceRemotingBondSerializationProvider : IServiceRemotingMessageSerializationProvider
    {
        public IBufferPoolManager BufferPoolManager { get; } = new BufferPoolManager();

        public IServiceRemotingMessageBodyFactory CreateMessageBodyFactory() => new BondMessageFactory();

        public IServiceRemotingRequestMessageBodySerializer CreateRequestMessageSerializer(Type serviceInterfaceType, IEnumerable<Type> requestBodyTypes)
            => new ServiceRemotingRequestBondMessageBodySerializer(BufferPoolManager, requestBodyTypes.ToList());

        public IServiceRemotingResponseMessageBodySerializer CreateResponseMessageSerializer(Type serviceInterfaceType, IEnumerable<Type> responseBodyTypes)
            => new ServiceRemotingResponseBondMessageBodySerializer(responseBodyTypes);
    }
}
