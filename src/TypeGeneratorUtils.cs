using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Bond;
using Sigil.NonGeneric;

namespace ServiceFabric.Remoting.Bond
{
    internal static class TypeGeneratorUtils
    {
        public static FieldBuilder AddBondProperty(TypeBuilder typeBuilder, string propName, Type propType, int propBondId)
        {
            var backingFieldBuilder = typeBuilder.DefineField($"_{propName}", propType, FieldAttributes.Private);
            var propBuilder = typeBuilder.DefineProperty(propName, PropertyAttributes.HasDefault, propType, null);

            // Define the "get" accessor method for the property.
            var getterMethod = Emit.BuildInstanceMethod(
                propType,
                Type.EmptyTypes,
                typeBuilder,
                $"get_{propName}",
                Constants.PropertyMethodAttributes);
            getterMethod.LoadArgument(0);
            getterMethod.LoadField(backingFieldBuilder);
            getterMethod.Return();
            var getterMethodBuilder = getterMethod.CreateMethod();

            // Define the "set" accessor method for the property.
            var setterMethod = Emit.BuildInstanceMethod(
                typeof(void),
                new[] { propType },
                typeBuilder,
                $"set_{propName}",
                Constants.PropertyMethodAttributes);
            setterMethod.LoadArgument(0);
            setterMethod.LoadArgument(1);
            setterMethod.StoreField(backingFieldBuilder);
            setterMethod.Return();
            var setterMethodBuilder = setterMethod.CreateMethod();

            propBuilder.SetGetMethod(getterMethodBuilder);
            propBuilder.SetSetMethod(setterMethodBuilder);

            // Add Bond.Id and Bond.Required attributes.
            propBuilder.SetCustomAttribute(new CustomAttributeBuilder(Constants.BondRequiredAttributeConstructor, Array.Empty<object>()));
            propBuilder.SetCustomAttribute(new CustomAttributeBuilder(Constants.BondIdAttributeConstructor, new object[] { (ushort)propBondId }));
            return backingFieldBuilder;
        }

        public static Dictionary<Type, BondTypeConverter> LoadConverters(Type converterType)
        {
            var converterMethods = converterType
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where((method) => method.Name == "Convert" && method.GetParameters().Length == 2 && method.ReturnType != typeof(void))

            var typesToConvert = converterMethods
                .Select((method) => method.GetParameters()[0].ParameterType)
                .Where((type) => type.GetBondDataType() == BondDataType.BT_UNAVAILABLE);

            var converters = new Dictionary<Type, BondTypeConverter>
        }
    }
}
