﻿using System.Collections.Generic;

namespace ServiceFabric.Bond.Remoting
{
    internal static class EnumerableExtensions
    {
        public static List<T> ToOrAsList<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable is not List<T> list)
            {
                return new List<T>(enumerable);
            }

            return list;
        }
    }
}
