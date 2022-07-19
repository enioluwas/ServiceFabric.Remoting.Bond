// --------------------------------------------------------------------------------
// <copyright file="BondResponseMessageBodyTypeGenerator.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace Microsoft.ServiceFabric.Services.Remoting.V2.Bond
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Threading;
    using Microsoft.ServiceFabric.Services.Remoting.V2;
    using Sigil.NonGeneric;

    internal sealed class BondResponseMessageBodyTypeGenerator
    {
        private static int TypeIdCounter;
        private const string GeneratedTypeSuffix = "_GeneratedResponse";

        public static BondResponseMessageBodyTypeGenerator Instance = new BondResponseMessageBodyTypeGenerator();

        private readonly CustomAttributeBuilder bondSchemaCustomAttribute;
        private readonly MethodInfo responseGetMethod;
        private readonly MethodInfo responseSetMethod;

        private BondResponseMessageBodyTypeGenerator()
        {
            this.bondSchemaCustomAttribute = new CustomAttributeBuilder(Constants.BondSchemaAttributeConstructor, Array.Empty<object>());
            this.responseGetMethod = typeof(IServiceRemotingResponseMessageBody).GetMethod(nameof(IServiceRemotingResponseMessageBody.Get));
            this.responseSetMethod = typeof(IServiceRemotingResponseMessageBody).GetMethod(nameof(IServiceRemotingResponseMessageBody.Set));
        }

        public BondGeneratedResponseType Generate(Type responseType)
        {
            var typeBuilder = Constants.GeneratedModuleBuilder.DefineType(this.GenerateTypeName(responseType));
            typeBuilder.SetCustomAttribute(this.bondSchemaCustomAttribute);
            var responseField = TypeGeneratorUtils.AddBondProperty(typeBuilder, "Response", responseType, 0);
            this.AddConstructors(typeBuilder, responseField);
            this.AddResponseInterfaceDefinition(typeBuilder, responseField);
            var generatedType = typeBuilder.CreateType();
            var typeInstanceFactory = this.BuildInstanceFactory(generatedType);
            return new BondGeneratedResponseType { Type = generatedType, InstanceFactory = typeInstanceFactory };
        }

        private void AddConstructors(TypeBuilder typeBuilder, FieldBuilder responseField)
        {
            var responseType = responseField.FieldType;

            // Add typed parameter constructor: new Generated(ResponseType response)
            var typedConstructor = Emit.BuildConstructor(new[] { responseType }, typeBuilder, MethodAttributes.Public);
            typedConstructor.LoadArgument(0);
            typedConstructor.LoadArgument(1);
            typedConstructor.StoreField(responseField);
            typedConstructor.Return();
            typedConstructor.CreateConstructor();

            // Add untyped parameter constructor: new Generated(object response)
            var untypedConstructor = Emit.BuildConstructor(new[] { typeof(object) }, typeBuilder, MethodAttributes.Public);
            untypedConstructor.LoadArgument(0);
            untypedConstructor.LoadArgument(1);
            if (!responseType.IsClass)
            {
                untypedConstructor.UnboxAny(responseType);
            }
            else if (responseType != typeof(object))
            {
                untypedConstructor.CastClass(responseType);
            }
            untypedConstructor.StoreField(responseField);
            untypedConstructor.Return();
            untypedConstructor.CreateConstructor();

            // Add parameterless constructor: new Generated()
            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
        }

        private Func<IServiceRemotingResponseMessageBody, object> BuildInstanceFactory(Type generatedType)
        {
            var response = Expression.Parameter(typeof(IServiceRemotingResponseMessageBody), "response");

            var generatedTypeConstructor = generatedType.GetConstructor(new[] { typeof(object) });
            // Generate a lambda equivalent to (response) => new Generated(response.Get(typeof(object)))
            var result = Expression.Lambda<Func<IServiceRemotingResponseMessageBody, object>>(
                Expression.New(generatedTypeConstructor,
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
            var setterMethod = Emit.BuildInstanceMethod(
                returnType: typeof(void),
                parameterTypes: new[] { typeof(object) },
                typeBuilder,
                name: this.responseSetMethod.Name,
                attributes: MethodAttributes.Public | MethodAttributes.Virtual);

            setterMethod.LoadArgument(0);
            setterMethod.LoadArgument(1);

            var responseType = responseField.FieldType;
            if (!responseType.IsClass)
            {
                setterMethod.UnboxAny(responseType);
            }
            else if (responseType != typeof(object))
            {
                setterMethod.CastClass(responseType);
            }

            setterMethod.StoreField(responseField);
            setterMethod.Return();
            var setterMethodBuilder = setterMethod.CreateMethod();
            typeBuilder.DefineMethodOverride(setterMethodBuilder, this.responseSetMethod);
        }

        private void AddResponseGetMethod(TypeBuilder typeBuilder, FieldBuilder responseField)
        {
            var getterMethod = Emit.BuildInstanceMethod(
                returnType: typeof(object),
                parameterTypes: new[] { typeof(Type) },
                typeBuilder,
                name: this.responseGetMethod.Name,
                attributes: MethodAttributes.Public | MethodAttributes.Virtual);

            getterMethod.LoadArgument(0);
            getterMethod.LoadField(responseField);
            var responseType = responseField.FieldType;
            if (!responseType.IsClass)
            {
                getterMethod.Box(responseType);
            }

            getterMethod.Return();
            var getterMethodBuilder = getterMethod.CreateMethod();
            typeBuilder.DefineMethodOverride(getterMethodBuilder, this.responseGetMethod);
        }
    }
}
