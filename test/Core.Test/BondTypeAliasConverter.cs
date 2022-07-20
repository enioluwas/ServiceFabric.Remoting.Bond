using System;

namespace ServiceFabric.Bond.Remoting.Test
{
    internal static class BondTypeAliasConverter
    {
        public static long Convert(TimeSpan value, long unused) => value.Ticks;

        public static TimeSpan Convert(long value, TimeSpan unused) => TimeSpan.FromTicks(value);
    }
}
