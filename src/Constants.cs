using System;
using System.Reflection;
using System.Reflection.Emit;
using Bond;

namespace ServiceFabric.Remoting.Bond
{
    internal static class Constants
    {
        public static readonly ConstructorInfo BondIdAttributeConstructor;
        public static readonly ConstructorInfo BondRequiredAttributeConstructor;
        public static readonly ConstructorInfo BondSchemaAttributeConstructor;
        public static readonly AssemblyBuilder GeneratedAssemblyBuilder;
        public static readonly ModuleBuilder GeneratedModuleBuilder;

        public const string GeneratedAssemblyName = "Kronox.WebClients.Remoting.BondGenerated";
        public static readonly MethodAttributes PropertyMethodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

        static Constants()
        {
            GeneratedAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(GeneratedAssemblyName), AssemblyBuilderAccess.Run);
            GeneratedModuleBuilder = GeneratedAssemblyBuilder.DefineDynamicModule(GeneratedAssemblyName);
            BondIdAttributeConstructor = typeof(IdAttribute).GetConstructor(new[] { typeof(ushort) });
            BondRequiredAttributeConstructor = typeof(RequiredAttribute).GetConstructor(Type.EmptyTypes);
            BondSchemaAttributeConstructor = typeof(SchemaAttribute).GetConstructor(Type.EmptyTypes);
        }
    }
}
