using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceFabric.Remoting.Bond
{
    internal static class BondTypeAliasConverter
    {
        /// <summary>
        /// Converts a <see cref="Guid"/> to a <see cref="string"/> for storage in a Bond document.
        /// </summary>
        public static string Convert(Guid value, string unused)
        {
            return value.ToString();
        }

        /// <summary>
        /// Converts a <see cref="string"/> to a <see cref="Guid"/>.
        /// Used to initialize a <see cref="Guid"/> from a Bond document.
        /// </summary>
        public static Guid Convert(string value, Guid unused)
        {
            return Guid.Parse(value);
        }

        /// <summary>
        /// Converts a <see cref="TimeSpan"/> to a <see cref="uint"/>.
        /// Used to store a <see cref="TimeSpan"/> in a Bond document.
        /// </summary>
        public static long Convert(TimeSpan value, long unused)
        {
            return value.Ticks;
        }

        /// <summary>
        /// Converts a <see cref="long"/> to a <see cref="TimeSpan"/>.
        /// Used to initialize a <see cref="TimeSpan"/> from a Bond document.
        /// </summary>
        public static TimeSpan Convert(long value, TimeSpan unused)
        {
            return TimeSpan.FromTicks(value);
        }
    }
}
