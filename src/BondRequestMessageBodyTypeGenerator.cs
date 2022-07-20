using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Sigil.NonGeneric;

namespace ServiceFabric.Bond.Remoting
{
    internal sealed class BondRequestMessageBodyTypeGenerator
    {
        private static int TypeIdCounter = 0;
        private const string GeneratedFieldPrefix = "Parameter_";
        private const string GeneratedTypeSuffix = "_GeneratedRequest";

        public static BondRequestMessageBodyTypeGenerator Instance = new BondRequestMessageBodyTypeGenerator();

        private readonly CustomAttributeBuilder bondSchemaCustomAttribute;
        private readonly MethodInfo requestGetParameterMethod;
        private readonly MethodInfo requestSetParameterMethod;

        private BondRequestMessageBodyTypeGenerator()
        {
            this.bondSchemaCustomAttribute = new CustomAttributeBuilder(Constants.BondSchemaAttributeConstructor, Array.Empty<object>());
            this.requestGetParameterMethod = typeof(IServiceRemotingRequestMessageBody).GetMethod(nameof(IServiceRemotingRequestMessageBody.GetParameter));
            this.requestSetParameterMethod = typeof(IServiceRemotingRequestMessageBody).GetMethod(nameof(IServiceRemotingRequestMessageBody.SetParameter));
        }

        public BondGeneratedRequestType Generate(Type[] requestTypes)
        {
            var typeBuilder = Constants.GeneratedModuleBuilder.DefineType(this.GenerateTypeName(requestTypes));
            typeBuilder.SetCustomAttribute(this.bondSchemaCustomAttribute);
            var generatedFields = this.AddBondProperties(typeBuilder, requestTypes);
            this.AddConstructors(typeBuilder, requestTypes, generatedFields);
            this.AddRequestInterfaceDefinition(typeBuilder, generatedFields);
            var generatedType = typeBuilder.CreateType();
            var typeInstanceFactory = this.BuildInstanceFactory(generatedType, requestTypes);
            return new BondGeneratedRequestType { Type = generatedType, InstanceFactory = typeInstanceFactory };
        }

        private Func<IServiceRemotingRequestMessageBody, object> BuildInstanceFactory(Type generatedType, Type[] requestTypes)
        {
            var request = Expression.Parameter(typeof(IServiceRemotingRequestMessageBody), "request");
            var generatedTypeConstructor = generatedType.GetConstructor(requestTypes);

            var fieldGetters = new Expression[requestTypes.Length];
            for (int i = 0; i < requestTypes.Length; i++)
            {
                // (Type)request.GetParameter(i, null, typeof(void));
                var getParameter = Expression.Call(
                    request,
                    this.requestGetParameterMethod,
                    Expression.Constant(i),
                    Expression.Constant(string.Empty, typeof(string)),
                    Expression.Constant(typeof(void), typeof(Type)));
                fieldGetters[i] = Expression.Convert(getParameter, requestTypes[i]);
            }

            var generatedRequest = Expression.New(generatedTypeConstructor, fieldGetters);

            // Generate a lambda like (request) => new Generated(request.GetParameter(1, null, typeof(void)), ... request.GetParameter(n, null, typeof(void)))
            var result = Expression.Lambda<Func<IServiceRemotingRequestMessageBody, object>>(generatedRequest, request);
            return result.Compile();
        }

        private void AddRequestInterfaceDefinition(TypeBuilder typeBuilder, FieldBuilder[] generatedFields)
        {
            typeBuilder.AddInterfaceImplementation(typeof(IServiceRemotingRequestMessageBody));
            this.AddRequestGetParameterMethod(typeBuilder, generatedFields);
            this.AddRequestSetParameterMethod(typeBuilder, generatedFields);
        }

        private void AddRequestGetParameterMethod(TypeBuilder typeBuilder, FieldBuilder[] generatedFields)
        {
            var getterMethod = Emit.BuildInstanceMethod(
                returnType: typeof(object),
                parameterTypes: new[] { typeof(int), typeof(string), typeof(Type) },
                typeBuilder,
                this.requestGetParameterMethod.Name,
                MethodAttributes.Public | MethodAttributes.Virtual);

            var labels = new Sigil.Label[generatedFields.Length];
            for (var i = 0; i < labels.Length; i++)
            {
                labels[i] = getterMethod.DefineLabel();
            }

            getterMethod.LoadArgument(1);
            getterMethod.Switch(labels);
            getterMethod.LoadArgument(2);
            getterMethod.NewObject<ArgumentOutOfRangeException, string>();
            getterMethod.Throw();

            for (var i = 0; i < generatedFields.Length; i++)
            {
                getterMethod.MarkLabel(labels[i]);
                getterMethod.LoadArgument(0);
                getterMethod.LoadField(generatedFields[i]);

                var fieldType = generatedFields[i].FieldType;
                if (!fieldType.IsClass)
                {
                    getterMethod.Box(fieldType);
                }
                getterMethod.Return();
            }

            var getterMethodBuilder = getterMethod.CreateMethod();
            typeBuilder.DefineMethodOverride(getterMethodBuilder, this.requestGetParameterMethod);
        }

        private void AddRequestSetParameterMethod(TypeBuilder typeBuilder, FieldBuilder[] generatedFields)
        {
            var setterMethod = Emit.BuildInstanceMethod(
                returnType: typeof(void),
                parameterTypes: new[] { typeof(int), typeof(string), typeof(object) },
                typeBuilder,
                this.requestSetParameterMethod.Name,
                MethodAttributes.Public | MethodAttributes.Virtual);

            var generatedFieldLabels = new Sigil.Label[generatedFields.Length];
            for (var i = 0; i < generatedFieldLabels.Length; i++)
            {
                generatedFieldLabels[i] = setterMethod.DefineLabel();
            }

            setterMethod.LoadArgument(1);
            setterMethod.Switch(generatedFieldLabels);
            setterMethod.LoadArgument(2);
            setterMethod.NewObject<ArgumentOutOfRangeException, string>();
            setterMethod.Throw();

            for (var i = 0; i < generatedFields.Length; i++)
            {
                setterMethod.MarkLabel(generatedFieldLabels[i]);
                setterMethod.LoadArgument(0);
                setterMethod.LoadArgument(3);

                var fieldType = generatedFields[i].FieldType;
                if (!fieldType.IsClass)
                {
                    setterMethod.UnboxAny(fieldType);
                }
                else if (fieldType != typeof(object))
                {
                    setterMethod.CastClass(fieldType);
                }

                setterMethod.StoreField(generatedFields[i]);
                setterMethod.Return();
            }

            var setterMethodBuilder = setterMethod.CreateMethod();
            typeBuilder.DefineMethodOverride(setterMethodBuilder, this.requestSetParameterMethod);
        }

        private string GenerateTypeName(Type[] requestTypes)
        {
            Interlocked.Increment(ref TypeIdCounter);

            var typeNameBuilder = new StringBuilder();
            for (int i = 0; i < requestTypes.Length; i++)
            {
                var typeName = requestTypes[i].Name;

                // Remove generic generatedType annotation, e.g from "Dictionary`2"
                if (typeName.Length > 2 && typeName[^2] == '`')
                {
                    typeNameBuilder.Append(typeName[0..^3]);
                }
                else
                {
                    typeNameBuilder.Append(typeName);
                    if (i < requestTypes.Length - 1)
                    {
                        typeNameBuilder.Append('_');
                    }
                }
            }

            return typeNameBuilder.Append(GeneratedTypeSuffix).Append('_').Append(TypeIdCounter).ToString();
        }

        private FieldBuilder[] AddBondProperties(TypeBuilder typeBuilder, Type[] requestTypes)
        {
            var backingFields = new FieldBuilder[requestTypes.Length];

            for (int i = 0; i < requestTypes.Length; i++)
            {
                backingFields[i] = TypeGeneratorUtils.AddBondProperty(typeBuilder, $"{GeneratedFieldPrefix}{i}", requestTypes[i], i);
            }

            return backingFields;
        }

        private void AddConstructors(TypeBuilder typeBuilder, Type[] requestTypes, FieldBuilder[] generatedFields)
        {
            // Add typed parameter constructor: new Generated(RequestType1 param1, ..., RequestTypeN paramN)
            var typedConstructor = Emit.BuildConstructor(requestTypes, typeBuilder, MethodAttributes.Public);
            for (int i = 0; i < generatedFields.Length; i++)
            {
                typedConstructor.LoadArgument(0);
                typedConstructor.LoadArgument((ushort)(i + 1));
                typedConstructor.StoreField(generatedFields[i]);
            }

            typedConstructor.Return();
            typedConstructor.CreateConstructor();

            // Add untyped parameter constructor: new Generated(object param1, ..., object paramN)
            var untypedConstructor = Emit.BuildConstructor(
                generatedFields.Select(_ => typeof(object)).ToArray(),
                typeBuilder,
                MethodAttributes.Public);

            for (int i = 0; i < generatedFields.Length; i++)
            {
                var fieldType = generatedFields[i].FieldType;

                untypedConstructor.LoadArgument(0);
                untypedConstructor.LoadArgument((ushort)(i + 1));
                if (!fieldType.IsClass)
                {
                    untypedConstructor.UnboxAny(fieldType);
                }
                else if (fieldType != typeof(object))
                {
                    untypedConstructor.CastClass(fieldType);
                }
                untypedConstructor.StoreField(generatedFields[i]);
            }

            untypedConstructor.Return();
            untypedConstructor.CreateConstructor();

            // Add parameterless constructor: new Generated()
            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
        }
    }
}
