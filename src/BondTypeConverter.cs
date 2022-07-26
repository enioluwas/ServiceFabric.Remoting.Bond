using System;
using System.Reflection;

namespace ServiceFabric.Remoting.Bond
{
    internal class BondTypeConverter
    {
        Type BondType { get; init; }
        Type NonBondType { get; init; }
        MethodInfo ConvertToBondType { get; init; }
        MethodInfo ConvertToNonBondType { get; init; }
    }
}
