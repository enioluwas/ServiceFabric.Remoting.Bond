using Bond;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneSkope.ServiceFabric.Remoting.Bond
{
    [Schema]
    internal sealed class BondEmptyResponseMessageBody : IServiceRemotingResponseMessageBody
    {
        public object Get(Type paramType) => null;

        public void Set(object response)
        {
        }
    }
}
