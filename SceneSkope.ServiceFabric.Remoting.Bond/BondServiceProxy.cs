using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneSkope.ServiceFabric.Remoting.Bond
{
    public static class BondServiceProxy
    {
        private static readonly ServiceProxyFactory _factory = new ServiceProxyFactory(c =>
            new FabricTransportServiceRemotingClientFactory(
                serializationProvider: new ServiceRemotingBondSerializationProvider()
            )
        );

        public static TServiceInterface Create<TServiceInterface>(
                    Uri serviceUri,
                    ServicePartitionKey partitionKey = null,
                    TargetReplicaSelector targetReplicaSelector = TargetReplicaSelector.Default,
                    string listenerName = null) where TServiceInterface : IService
        {
            return _factory.CreateServiceProxy<TServiceInterface>(
                serviceUri,
                partitionKey,
                targetReplicaSelector,
                listenerName);
        }
    }
}
