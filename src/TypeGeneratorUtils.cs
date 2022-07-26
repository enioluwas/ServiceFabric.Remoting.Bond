using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Sigil.NonGeneric;

namespace ServiceFabric.Remoting.Bond
{
    internal static class TypeGeneratorUtils
    {
        public static FieldBuilder AddBondProperty(TypeBuilder typeBuilder, string propName, Type propType, int propBondId, Dictionary<Type, BondTypeConverter> typeConverterMap)
        {
            var backingFieldBuilder = typeBuilder.DefineField($"_{propName}", propType, FieldAttributes.Private);

            Type bondPropType = propType;
            if (typeConverterMap.TryGetValue(propType, out var converter))
            {
                bondPropType = converter.BondType;
            }

            var propBuilder = typeBuilder.DefineProperty(propName, PropertyAttributes.HasDefault, bondPropType, null);

            // Define the "get" accessor method for the property.
            var getterMethod = Emit.BuildInstanceMethod(
                bondPropType,
                Type.EmptyTypes,
                typeBuilder,
                $"get_{propName}",
                Constants.PropertyMethodAttributes,
                doVerify: Constants.VerifyIL);
            getterMethod.LoadArgument(0);
            getterMethod.LoadField(backingFieldBuilder);

            if (converter != null)
            {
                using var local = getterMethod.DeclareLocal(bondPropType);
                getterMethod.LoadLocal(local);
                getterMethod.Call(converter.ConvertToBondType);
            }

            getterMethod.Return();
            var getterMethodBuilder = getterMethod.CreateMethod();

            // Define the "set" accessor method for the property.
            var setterMethod = Emit.BuildInstanceMethod(
                typeof(void),
                new[] { bondPropType },
                typeBuilder,
                $"set_{propName}",
                Constants.PropertyMethodAttributes,
                doVerify: Constants.VerifyIL);
            setterMethod.LoadArgument(0);
            setterMethod.LoadArgument(1);

            if (converter != null)
            {
                using var local = setterMethod.DeclareLocal(propType);
                setterMethod.LoadLocal(local);
                setterMethod.Call(converter.ConvertToNonBondType);
            }

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
            var methods = converterType.GetMethods(BindingFlags.Public | BindingFlags.Static);
            var result = new Dictionary<Type, BondTypeConverter>();

            foreach (var method in methods)
            {
                if (method.Name != "Convert")
                {
                    continue;
                }

                var parameters = method.GetParameters();
                if (parameters.Length != 2)
                {
                    // Not a converter method.
                    continue;
                }

                var convertFromType = parameters[0].ParameterType;
                var convertToType = parameters[1].ParameterType;

                if (result.TryGetValue(convertToType, out var converter) && converter.ConvertToNonBondType == null)
                {
                    converter.ConvertToNonBondType = method;
                }
                else if (result.TryGetValue(convertFromType, out converter) && converter.ConvertToBondType == null)
                {
                    converter.ConvertToBondType = method;
                }
                else if (!convertFromType.IsBondType() && convertToType.IsBondType())
                {
                    result[convertFromType] = new BondTypeConverter
                    {
                        BondType = convertToType,
                        NonBondType = convertFromType,
                        ConvertToBondType = method
                    };
                }
                else if (convertFromType.IsBondType() && !convertToType.IsBondType())
                {
                    result[convertToType] = new BondTypeConverter
                    {
                        BondType = convertFromType,
                        NonBondType = convertToType,
                        ConvertToNonBondType = method
                    };
                }
            }

            return result;
        }
    }
}
