using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Microsoft.ServiceFabric.Services.Remoting.V2.Bond
{
    internal static class TypeGeneratorUtils
    {
        public static FieldBuilder AddBondProperty(TypeBuilder typeBuilder, string propName, Type propType, int propBondId)
        {
            var backingFieldBuilder = typeBuilder.DefineField($"_{propName}", propType, FieldAttributes.Private);
            var propBuilder = typeBuilder.DefineProperty(propName, PropertyAttributes.HasDefault, propType, null);

            // Define the "get" accessor method for the property.
            var getterMethodBuilder = typeBuilder.DefineMethod($"get_{propName}", Constants.PropertyMethodAttributes, propType, Type.EmptyTypes);
            var getterMethodIL = getterMethodBuilder.GetILGenerator();
            getterMethodIL.Emit(OpCodes.Ldarg_0);
            getterMethodIL.Emit(OpCodes.Ldfld, backingFieldBuilder);
            getterMethodIL.Emit(OpCodes.Ret);

            // Define the "set" accessor method for the property.
            var setterMethodBuilder = typeBuilder.DefineMethod($"set_{propName}", Constants.PropertyMethodAttributes, null, new[] { propType });
            ILGenerator setterMethodIL = setterMethodBuilder.GetILGenerator();
            setterMethodIL.Emit(OpCodes.Ldarg_0);
            setterMethodIL.Emit(OpCodes.Ldarg_1);
            setterMethodIL.Emit(OpCodes.Stfld, backingFieldBuilder);
            setterMethodIL.Emit(OpCodes.Ret);

            propBuilder.SetGetMethod(getterMethodBuilder);
            propBuilder.SetSetMethod(setterMethodBuilder);

            // Add Bond.Id and Bond.Required attributes.
            propBuilder.SetCustomAttribute(new CustomAttributeBuilder(Constants.BondRequiredAttributeConstructor, null));
            propBuilder.SetCustomAttribute(new CustomAttributeBuilder(Constants.BondIdAttributeConstructor, new object[] { propBondId }));
            return backingFieldBuilder;
        }
    }
}
