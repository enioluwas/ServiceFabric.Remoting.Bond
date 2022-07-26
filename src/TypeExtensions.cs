using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bond;

namespace ServiceFabric.Remoting.Bond
{
    internal static class TypeExtensions
    {
        public static bool IsBondType(this Type type) => type.GetBondDataType() != BondDataType.BT_UNAVAILABLE;
    }
}
