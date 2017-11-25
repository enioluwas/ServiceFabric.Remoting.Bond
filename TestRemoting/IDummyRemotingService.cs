using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace TestRemoting
{
    internal interface IDummyRemotingService : IService
    {
        Task BasicMethodAsync(string param1, int param2, ArraySegment<byte> param3);
    }
}
