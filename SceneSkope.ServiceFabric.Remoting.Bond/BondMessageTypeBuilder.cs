using Bond;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Sigil.NonGeneric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace SceneSkope.ServiceFabric.Remoting.Bond
{
    public static class BondMessageTypeBuilder
    {
        private const string _namespace = "generated";
        private static int _id = 0;

        public static Type CreateResponseMessageBody(Type responseType)
        {
            var typeBuilder = CreateBuilder();

            var parameterTypes = new[] { responseType };
            CreateProperties(typeBuilder, parameterTypes, out var fields, out var setters, out var getters);

            CreateDefaultConstructor(typeBuilder);
            CreateFullConstructor(typeBuilder, parameterTypes, fields);
            CreateObjectConstructor(typeBuilder, parameterTypes, fields);

            typeBuilder.AddInterfaceImplementation(typeof(IServiceRemotingResponseMessageBody));
            BuildSet(responseType, typeBuilder, fields);
            BuildGet(responseType, typeBuilder, fields);

            return typeBuilder.CreateType();
        }

        private static void BuildSet(Type responseType, TypeBuilder typeBuilder, FieldBuilder[] fields)
        {
            var set = Emit.BuildInstanceMethod(typeof(void), new[] { typeof(object) }, typeBuilder, "Set", MethodAttributes.Public | MethodAttributes.Virtual);
            set.LoadArgument(0);
            set.LoadArgument(1);
            if (!responseType.IsClass)
            {
                set.UnboxAny(responseType);
            }
            else if (responseType != typeof(object))
            {
                set.CastClass(responseType);
            }
            set.StoreField(fields[0]);
            set.Return();
            var setter = set.CreateMethod();
            typeBuilder.DefineMethodOverride(setter, typeof(IServiceRemotingResponseMessageBody).GetMethod("Set"));
        }

        private static void BuildGet(Type responseType, TypeBuilder typeBuilder, FieldBuilder[] fields)
        {
            var get = Emit.BuildInstanceMethod(typeof(object), new[] { typeof(Type) }, typeBuilder, "Get", MethodAttributes.Public | MethodAttributes.Virtual);
            get.LoadArgument(0);
            get.LoadField(fields[0]);
            if (!responseType.IsClass)
            {
                get.Box(responseType);
            }
            get.Return();
            var getter = get.CreateMethod();
            typeBuilder.DefineMethodOverride(getter, typeof(IServiceRemotingResponseMessageBody).GetMethod("Get"));
        }

        public static Type CreateRequestMessageBody(IEnumerable<Type> propertyTypes)
        {
            var typeBuilder = CreateBuilder();

            var parameterTypes = propertyTypes.ToArray();
            CreateProperties(typeBuilder, parameterTypes, out var fields, out var setters, out var getters);

            CreateDefaultConstructor(typeBuilder);
            CreateFullConstructor(typeBuilder, parameterTypes, fields);
            CreateObjectConstructor(typeBuilder, parameterTypes, fields);

            typeBuilder.AddInterfaceImplementation(typeof(IServiceRemotingRequestMessageBody));
            BuildSetParameter(typeBuilder, parameterTypes, fields);
            BuildGetParameter(typeBuilder, parameterTypes, fields);

            return typeBuilder.CreateType();
        }

        private static void BuildGetParameter(TypeBuilder typeBuilder, Type[] parameterTypes, FieldBuilder[] fields)
        {
            var get = Emit.BuildInstanceMethod(typeof(object), new[] { typeof(int), typeof(string), typeof(Type) }, typeBuilder, "GetParameter", MethodAttributes.Public | MethodAttributes.Virtual);
            var labels = new Sigil.Label[parameterTypes.Length];
            for (var i = 0; i < labels.Length; i++)
            {
                labels[i] = get.DefineLabel();
            }

            get.LoadArgument(1);
            get.Switch(labels);
            get.LoadArgument(2);
            get.NewObject<ArgumentOutOfRangeException, string>();
            get.Throw();

            for (var i = 0; i < parameterTypes.Length; i++)
            {
                get.MarkLabel(labels[i]);
                get.LoadArgument(0);
                get.LoadField(fields[i]);

                var parameterType = parameterTypes[i];
                if (!parameterType.IsClass)
                {
                    get.Box(parameterType);
                }
                get.Return();
            }

            var getter = get.CreateMethod();
            typeBuilder.DefineMethodOverride(getter, typeof(IServiceRemotingRequestMessageBody).GetMethod("GetParameter"));
        }

        private static void BuildSetParameter(TypeBuilder typeBuilder, Type[] parameterTypes, FieldBuilder[] fields)
        {
            var set = Emit.BuildInstanceMethod(typeof(void), new[] { typeof(int), typeof(string), typeof(object) }, typeBuilder, "SetParameter", MethodAttributes.Public | MethodAttributes.Virtual);

            var labels = new Sigil.Label[parameterTypes.Length];
            for (var i = 0; i < labels.Length; i++)
            {
                labels[i] = set.DefineLabel();
            }

            set.LoadArgument(1);
            set.Switch(labels);
            set.LoadArgument(2);
            set.NewObject<ArgumentOutOfRangeException, string>();
            set.Throw();

            for (var i = 0; i < parameterTypes.Length; i++)
            {
                set.MarkLabel(labels[i]);
                set.LoadArgument(0);
                set.LoadArgument(3);
                var parameterType = parameterTypes[i];
                if (!parameterType.IsClass)
                {
                    set.UnboxAny(parameterType);
                }
                else if (parameterType != typeof(object))
                {
                    set.CastClass(parameterType);
                }
                set.StoreField(fields[i]);
                set.Return();
            }

            var setter = set.CreateMethod();
            typeBuilder.DefineMethodOverride(setter, typeof(IServiceRemotingRequestMessageBody).GetMethod("SetParameter"));
        }

        public static Type CreateObject(IEnumerable<Type> propertyTypes)
        {
            var typeBuilder = CreateBuilder();

            var parameterTypes = propertyTypes.ToArray();
            CreateProperties(typeBuilder, parameterTypes, out var fields, out var setters, out var getters);

            CreateDefaultConstructor(typeBuilder);
            CreateFullConstructor(typeBuilder, parameterTypes, fields);
            CreateObjectConstructor(typeBuilder, parameterTypes, fields);

            return typeBuilder.CreateType();
        }

        private static TypeBuilder CreateBuilder()
        {
            var id = Interlocked.Increment(ref _id);
            var name = $"generated_{id}";
            var asm = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(name), AssemblyBuilderAccess.Run);

            var module = asm.DefineDynamicModule("module");

            var typeBuilder = module.DefineType($"{_namespace}.{name}");
            typeBuilder.SetCustomAttribute(new CustomAttributeBuilder(typeof(SchemaAttribute).GetConstructor(Type.EmptyTypes), Array.Empty<object>()));
            return typeBuilder;
        }

        private static FieldBuilder[] CreateProperties(TypeBuilder typeBuilder, Type[] parameterTypes, out FieldBuilder[] fields, out MethodBuilder[] setters, out MethodBuilder[] getters)
        {
            fields = new FieldBuilder[parameterTypes.Length];
            setters = new MethodBuilder[parameterTypes.Length];
            getters = new MethodBuilder[parameterTypes.Length];
            for (var index = 0; index < parameterTypes.Length; index++)
            {
                var propertyType = parameterTypes[index];
                var backingField = typeBuilder.DefineField($"_{index}", propertyType, FieldAttributes.Private);
                fields[index] = backingField;

                Type bondPropertyType;
                if (TryFindBondTypeAliasConverter(propertyType, out var convertFromTransport, out var convertToTransport))
                {
                    bondPropertyType = convertToTransport.ReturnType;
                }
                else
                {
                    bondPropertyType = propertyType;
                }

                var property = typeBuilder.DefineProperty($"{index}", PropertyAttributes.HasDefault, bondPropertyType, null);

                var getter = Emit.BuildInstanceMethod(bondPropertyType, Type.EmptyTypes, typeBuilder, $"get_{index}",
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig);
                getter.LoadArgument(0);
                getter.LoadField(backingField);
                if (convertToTransport != null)
                {
                    using (var local = getter.DeclareLocal(bondPropertyType))
                    {
                        getter.LoadLocal(local);
                        getter.Call(convertToTransport);
                    }
                }
                getter.Return();
                var getterMethod = getter.CreateMethod();
                property.SetGetMethod(getterMethod);
                getters[index] = getterMethod;

                var setter = Emit.BuildInstanceMethod(typeof(void), new[] { bondPropertyType }, typeBuilder, $"set_{index}",
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig);
                setter.LoadArgument(0);
                setter.LoadArgument(1);
                if (convertFromTransport != null)
                {
                    using (var local = setter.DeclareLocal(propertyType))
                    {
                        setter.LoadLocal(local);
                        setter.Call(convertFromTransport);
                    }
                }
                setter.StoreField(backingField);
                setter.Return();
                var setterMethod = setter.CreateMethod();
                property.SetSetMethod(setterMethod);
                setters[index] = setterMethod;

                property.SetCustomAttribute(new CustomAttributeBuilder(typeof(RequiredAttribute).GetConstructor(Type.EmptyTypes), Array.Empty<object>()));
                property.SetCustomAttribute(new CustomAttributeBuilder(typeof(IdAttribute).GetConstructor(new[] { typeof(ushort) }), new object[] { (ushort)index }));
            }

            return fields;
        }

        private static void CreateDefaultConstructor(TypeBuilder typeBuilder)
        {
            var constructor = Emit.BuildConstructor(Type.EmptyTypes, typeBuilder, MethodAttributes.Public);
            constructor.Return();
            constructor.CreateConstructor();
        }

        private static void CreateFullConstructor(TypeBuilder typeBuilder, Type[] parameterTypes, FieldBuilder[] fields)
        {
            var constructor = Emit.BuildConstructor(parameterTypes, typeBuilder, MethodAttributes.Public);
            for (var index = 0; index < parameterTypes.Length; index++)
            {
                constructor.LoadArgument(0);
                constructor.LoadArgument((ushort)(index + 1));
                constructor.StoreField(fields[index]);
            }
            constructor.Return();
            constructor.CreateConstructor();
        }

        private static void CreateObjectConstructor(TypeBuilder typeBuilder, Type[] parameterTypes, FieldBuilder[] fields)
        {
            var objectTypes = new Type[parameterTypes.Length];
            for (var i = 0; i < objectTypes.Length; i++)
            {
                objectTypes[i] = typeof(object);
            }

            var constructor = Emit.BuildConstructor(objectTypes, typeBuilder, MethodAttributes.Public);
            for (var index = 0; index < parameterTypes.Length; index++)
            {
                var parameterType = parameterTypes[index];
                constructor.LoadArgument(0);
                constructor.LoadArgument((ushort)(index + 1));
                if (!parameterType.IsClass)
                {
                    constructor.UnboxAny(parameterType);
                }
                else if (parameterType != typeof(object))
                {
                    constructor.CastClass(parameterType);
                }
                constructor.StoreField(fields[index]);
            }
            constructor.Return();
            constructor.CreateConstructor();
        }

        private static bool TryFindBondTypeAliasConverter(Type propertyType, out MethodInfo convertFromTransport, out MethodInfo convertToTransport)
        {
            if (propertyType.IsPrimitive)
            {
                convertFromTransport = null;
                convertToTransport = null;
                return false;
            }
            var name = propertyType.Namespace + ".BondTypeAliasConverter" + propertyType.AssemblyQualifiedName.Substring(propertyType.Name.Length);
            var converterType = propertyType.Assembly.GetType(name, false);
            if (converterType != null)
            {
                if (TryFindBondTypeAliasConverter(propertyType, converterType, out convertFromTransport, out convertToTransport))
                {
                    return true;
                }
            }
            return TryFindBondTypeAliasConverter(propertyType, typeof(BondTypeAliasConverter), out convertFromTransport, out convertToTransport);
        }

        private static bool TryFindBondTypeAliasConverter(Type propertyType, Type converterType, out MethodInfo convertFromTransport, out MethodInfo convertToTransport)
        {
            var methods = converterType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(mi => (mi.Name == "Convert")
                    && (mi.GetParameters().Length == 2)
                    && ((mi.ReturnType == propertyType) || (mi.GetParameters()[0].ParameterType == propertyType))
                )
                .ToList();

            if (methods.Count != 2)
            {
                convertFromTransport = null;
                convertToTransport = null;
                return false;
            }
            else
            {
                var first = methods[0];
                var second = methods[1];
                if (first.ReturnType == propertyType)
                {
                    convertFromTransport = first;
                    convertToTransport = second;
                }
                else
                {
                    convertFromTransport = second;
                    convertToTransport = first;
                }
                return true;
            }
        }
    }
}
