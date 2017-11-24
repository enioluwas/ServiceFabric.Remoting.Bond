using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneSkope.ServiceFabric.Remoting.Bond
{
    public static class CachedBondMessageTypeBuilder
    {
        private class ListTypeComparer : IEqualityComparer<IList<Type>>
        {
            public bool Equals(IList<Type> x, IList<Type> y)
            {
                if (x.Count != y.Count)
                {
                    return false;
                }
                for (var i = 0; i < x.Count; i++)
                {
                    if (!Type.Equals(x[i], y[i]))
                    {
                        return false;
                    }
                }
                return true;
            }

            public int GetHashCode(IList<Type> obj) => obj.GetHashCode();
        }

        private static ImmutableDictionary<Type, Type> _responseTypeMap = ImmutableDictionary<Type, Type>.Empty;
        private static ImmutableDictionary<IList<Type>, Type> _requestTypeMap = ImmutableDictionary<IList<Type>, Type>.Empty.WithComparers(new ListTypeComparer());

        public static Type GetOrAddResponseType(Type responseType) =>
            ImmutableInterlocked.GetOrAdd(ref _responseTypeMap, responseType, BondMessageTypeBuilder.CreateResponseMessageBody);

        public static Type GetOrAddRequestType(IList<Type> requestTypes) =>
            ImmutableInterlocked.GetOrAdd(ref _requestTypeMap, requestTypes, BondMessageTypeBuilder.CreateRequestMessageBody);
    }
}
