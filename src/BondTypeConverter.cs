using System;
using System.Reflection;

namespace ServiceFabric.Remoting.Bond
{
    internal class BondTypeConverter
    {
        public Type BondType { get; set; }
        public Type NonBondType { get; set; }
        public MethodInfo ConvertToBondType { get; set; }
        public MethodInfo ConvertToNonBondType { get; set; }
    }
}
