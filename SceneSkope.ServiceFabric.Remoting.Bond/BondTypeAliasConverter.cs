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

        public static long Convert(TimeSpan value, long unused) => value.Ticks;
        public static TimeSpan Convert(long value, TimeSpan unused) => new TimeSpan(value);

        public static ArraySegment<byte> Convert(System.Guid value, ArraySegment<byte> unused) => new ArraySegment<byte>(value.ToByteArray());

        public static System.Guid Convert(ArraySegment<byte> value, System.Guid unused)
        {
            var buffer = new byte[16];
            Buffer.BlockCopy(value.Array, value.Offset, buffer, 0, buffer.Length);
            return new Guid(buffer);
        }
#pragma warning restore RCS1163 // Unused parameter.
    }
}
