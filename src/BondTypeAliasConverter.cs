using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneSkope.ServiceFabric.Remoting.Bond
{
    public static class BondTypeAliasConverter
    {
#pragma warning disable RCS1163 // Unused parameter.
        public static long Convert(DateTime value, long unused) => value.Ticks;

        public static DateTime Convert(long value, DateTime unused) => new DateTime(value, DateTimeKind.Utc);
#pragma warning restore RCS1163 // Unused parameter.
    }
}
