using System;
using System.Collections.Generic;

namespace ServiceFabric.Remoting.Bond
{
    internal class TypeListEqualityComparer : IEqualityComparer<List<Type>>
    {
        public bool Equals(List<Type> x, List<Type> y)
        {
            if (x.Count != y.Count)
            {
                return false;
            }

            for (int i = 0; i < x.Count; i++)
            {
                if (x[i] != y[i])
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(List<Type> obj) => obj.GetHashCode();
    }
}
