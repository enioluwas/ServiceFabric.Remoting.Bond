// --------------------------------------------------------------------------------
// <copyright file="BondResponseMessageBodyTypeGenerator.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace Microsoft.ServiceFabric.Services.Remoting.V2.Bond
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Threading;
    using Microsoft.ServiceFabric.Services.Remoting.V2;
    using Sigil.NonGeneric;

    internal sealed class BondResponseMessageBodyTypeGenerator
    {
        private static int TypeIdCounter = 0;
        private const string GeneratedTypeSuffix = "_GeneratedResponse";

        public static BondResponseMessageBodyTypeGenerator Instance = new BondResponseMessageBodyTypeGenerator();

        private readonly CustomAttributeBuilder bondSchemaCustomAttribute;
        private readonly MethodInfo responseGetMethod;
        private readonly MethodInfo responseSetMethod;

        private BondResponseMessageBodyTypeGenerator()
        {
            this.bondSchemaCustomAttribute = new CustomAttributeBuilder(Constants.BondSchemaAttributeConstructor, null);
            this.responseGetMethod = typeof(IServiceRemotingResponseMessageBody).GetMethod(nameof(IServiceRemotingResponseMessageBody.Get));
            this.responseSetMethod = typeof(IServiceRemotingResponseMessageBody).GetMethod(nameof(IServiceRemotingResponseMessageBody.Set));
        }

        public BondGeneratedResponseType Generate(Type responseType)
        {
            var typeBuilder = Constants.GeneratedModuleBuilder.DefineType(this.GenerateTypeName(responseType));
            typeBuilder.SetCustomAttribute(this.bondSchemaCustomAttribute);
            var responseField = TypeGeneratorUtils.AddBondProperty(typeBuilder, "Response", responseType, 0);
            var generatedConstructors = this.AddConstructors(typeBuilder, responseField);
            this.AddResponseInterfaceDefinition(typeBuilder, responseField);
            var generatedType = typeBuilder.CreateType();
            var typeInstanceFactory = this.BuildInstanceFactory(generatedConstructors);
            return new BondGeneratedResponseType { Type = generatedType, InstanceFactory = typeInstanceFactory };
        }

        private GeneratedConstructors AddConstructors(TypeBuilder typeBuilder, FieldBuilder responseField)
        {
            var responseType = responseField.FieldType;

            // Add typed parameter constructor: new Generated(ResponseType response)
            var typedConstructor = Emit.BuildConstructor(new[] { responseType }, typeBuilder, MethodAttributes.Public);
            var typedConstructorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new[] { responseType });

            var typedConstructorIL = typedConstructorBuilder.GetILGenerator();
            typedConstructorIL.Emit(OpCodes.Ldarg_0);
            typedConstructorIL.Emit(OpCodes.Ldarg_1);
            typedConstructorIL.Emit(OpCodes.Stfld, responseField);
            typedConstructorIL.Emit(OpCodes.Ret);

            // Add untyped parameter constructor: new Generated(object response)
            var untypedConstructorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new[] { typeof(object) });

            var untypedConstructorIL = typedConstructorBuilder.GetILGenerator();
            untypedConstructorIL.Emit(OpCodes.Ldarg_0);
            untypedConstructorIL.Emit(OpCodes.Ldarg_1);
            if (!responseType.IsClass)
            {
                untypedConstructorIL.Emit(OpCodes.Unbox_Any, responseType);
            }
            else if (responseType != typeof(object))
            {
                untypedConstructorIL.Emit(OpCodes.Castclass, responseType);
            }
            untypedConstructorIL.Emit(OpCodes.Stfld, responseField);
            untypedConstructorIL.Emit(OpCodes.Ret);

            // Add parameterless constructor: new Generated()
            var parameterlessConstructorBuilder = typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

            return new GeneratedConstructors
            {
                Parameterless = parameterlessConstructorBuilder,
                Typed = typedConstructorBuilder,
                Untyped = untypedConstructorBuilder,
            };
        }

        private Func<IServiceRemotingResponseMessageBody, object> BuildInstanceFactory(GeneratedConstructors constructors)
        {
            var response = Expression.Parameter(typeof(IServiceRemotingResponseMessageBody), "response");

            // Generate a lambda equivalent to (response) => new Generated(response.Get(typeof(object)))
            var result = Expression.Lambda<Func<IServiceRemotingResponseMessageBody, object>>(
                Expression.New(constructors.Untyped,
                Expression.Call(response, this.responseGetMethod, Expression.Constant(typeof(object), typeof(Type)))),
                response);
            return result.Compile();
        }

        private string GenerateTypeName(Type responseType)
        {
            Interlocked.Increment(ref TypeIdCounter);

            var typeName = responseType.Name;

            // Remove generic generatedType annotation, e.g from "Dictionary`2"
            if (typeName.Length > 2 && typeName[^2] == '`')
            {
                return $"{typeName[0..^3]}{GeneratedTypeSuffix}_{TypeIdCounter}";
            }

            return $"{typeName}{GeneratedTypeSuffix}_{TypeIdCounter}";
        }

        private void AddResponseInterfaceDefinition(TypeBuilder typeBuilder, FieldBuilder responseField)
        {
            typeBuilder.AddInterfaceImplementation(typeof(IServiceRemotingResponseMessageBody));
            this.AddResponseGetMethod(typeBuilder, responseField);
            this.AddResponseSetMethod(typeBuilder, responseField);
        }

        private void AddResponseSetMethod(TypeBuilder typeBuilder, FieldBuilder responseField)
        {
            var setterMethodBuilder = typeBuilder.DefineMethod(
                name: this.responseSetMethod.Name,
                attributes: MethodAttributes.Public,
                returnType: typeof(void),
                parameterTypes: new[] { typeof(object) });

            var setterMethodIL = setterMethodBuilder.GetILGenerator();
            setterMethodIL.Emit(OpCodes.Ldarg_0);
            setterMethodIL.Emit(OpCodes.Ldarg_1);
            var responseType = responseField.FieldType;
            if (!responseType.IsClass)
            {
                setterMethodIL.Emit(OpCodes.Unbox_Any, responseType);
            }
            else if (responseField.FieldType != typeof(object))
            {
                setterMethodIL.Emit(OpCodes.Castclass, responseType);
            }

            setterMethodIL.Emit(OpCodes.Stfld, responseField);
            setterMethodIL.Emit(OpCodes.Ret);
            typeBuilder.DefineMethodOverride(setterMethodBuilder, this.responseSetMethod);
        }

        private void AddResponseGetMethod(TypeBuilder typeBuilder, FieldBuilder responseField)
        {
            var getterMethodBuilder = typeBuilder.DefineMethod(
                name: this.responseGetMethod.Name,
                attributes: MethodAttributes.Public,
                returnType: typeof(object),
                parameterTypes: new[] { typeof(Type) });

            var getterMethodIL = getterMethodBuilder.GetILGenerator();
            getterMethodIL.Emit(OpCodes.Ldarg_0);
            getterMethodIL.Emit(OpCodes.Ldfld, responseField);
            var responseType = responseField.FieldType;
            if (!responseType.IsClass)
            {
                getterMethodIL.Emit(OpCodes.Box, responseType);
            }

            getterMethodIL.Emit(OpCodes.Ret);
            typeBuilder.DefineMethodOverride(getterMethodBuilder, this.responseGetMethod);
        }
    }
}
