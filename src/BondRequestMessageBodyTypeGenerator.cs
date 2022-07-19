using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;

namespace Microsoft.ServiceFabric.Services.Remoting.V2.Bond
{
    internal sealed class BondRequestMessageBodyTypeGenerator
    {
        private static int TypeIdCounter = 0;
        private const string GeneratedFieldPrefix = "Parameter_";
        private const string GeneratedTypeSuffix = "_GeneratedRequest";

        public static BondRequestMessageBodyTypeGenerator Instance = new BondRequestMessageBodyTypeGenerator();

        private readonly CustomAttributeBuilder bondSchemaCustomAttribute;
        private readonly MethodAttributes propMethodAttributes;
        private readonly MethodInfo requestGetParameterMethod;
        private readonly MethodInfo requestSetParameterMethod;

        // TODO: Pass request type array instead of using new ones
        private BondRequestMessageBodyTypeGenerator()
        {
            this.bondSchemaCustomAttribute = new CustomAttributeBuilder(Constants.BondSchemaAttributeConstructor, null);
            this.propMethodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
            this.requestGetParameterMethod = typeof(IServiceRemotingRequestMessageBody).GetMethod(nameof(IServiceRemotingRequestMessageBody.GetParameter));
            this.requestSetParameterMethod = typeof(IServiceRemotingRequestMessageBody).GetMethod(nameof(IServiceRemotingRequestMessageBody.SetParameter));
        }

        public BondGeneratedRequestType Generate(Type[] requestTypes)
        {
            var typeBuilder = Constants.GeneratedModuleBuilder.DefineType(this.GenerateTypeName(requestTypes));
            typeBuilder.SetCustomAttribute(this.bondSchemaCustomAttribute);
            var generatedFields = this.AddBondProperties(typeBuilder, requestTypes);
            var generatedConstructors = this.AddConstructors(typeBuilder, generatedFields);
            this.AddRequestInterfaceDefinition(typeBuilder, generatedFields);
            var generatedType = typeBuilder.CreateType();
            var typeInstanceFactory = this.BuildInstanceFactory(generatedConstructors, generatedFields);
            return new BondGeneratedRequestType { Type = generatedType, InstanceFactory = typeInstanceFactory };
        }

        private Func<IServiceRemotingRequestMessageBody, object> BuildInstanceFactory(GeneratedConstructors constructors, FieldBuilder[] generatedFields)
        {
            var request = Expression.Parameter(typeof(IServiceRemotingRequestMessageBody), "request");

            var fieldGetters = new MethodCallExpression[generatedFields.Length];
            for (int i = 0; i < generatedFields.Length; i++)
            {
                // request.GetParameter(i, null, typeof(void));
                fieldGetters[i] = Expression.Call(
                    request,
                    this.requestGetParameterMethod,
                    Expression.Constant(i),
                    Expression.Constant(null),
                    Expression.Constant(typeof(void), typeof(Type)));
            }

            // Generate a lambda like (request) => new Generated(request.GetParameter(1, null, typeof(void)), ... request.GetParameter(n, null, typeof(void)))
            var result = Expression.Lambda<Func<IServiceRemotingRequestMessageBody, object>>(
                Expression.New(constructors.Untyped, fieldGetters),
                request);
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
            var getterMethodBuilder = typeBuilder.DefineMethod(
                name: this.requestGetParameterMethod.Name,
                attributes: MethodAttributes.Public,
                returnType: typeof(object),
                parameterTypes: new[] { typeof(int), typeof(string), typeof(Type) });

            var getterMethodIL = getterMethodBuilder.GetILGenerator();

            var labels = new Label[generatedFields.Length];
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i] = getterMethodIL.DefineLabel();
            }

            getterMethodIL.Emit(OpCodes.Ldarg_1);
            getterMethodIL.Emit(OpCodes.Switch, labels);
            getterMethodIL.Emit(OpCodes.Ldarg_2);

        }

        private void AddRequestSetParameterMethod(TypeBuilder typeBuilder, FieldBuilder[] generatedFields)
        {
            throw new NotImplementedException();
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

        private GeneratedConstructors AddConstructors(TypeBuilder typeBuilder, FieldBuilder[] generatedFields)
        {
            // Add typed parameter constructor: new Generated(RequestType1 param1, ..., RequestTypeN paramN)
            var typedConstructorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                generatedFields.Select(f => f.FieldType).ToArray());

            var typedConstructorIL = typedConstructorBuilder.GetILGenerator();
            for (int i = 0; i < generatedFields.Length; i++)
            {
                typedConstructorIL.Emit(OpCodes.Ldarg_0);
                typedConstructorIL.Emit(OpCodes.Ldarg_S, (ushort)i);
                typedConstructorIL.Emit(OpCodes.Stfld, generatedFields[i]);
                typedConstructorIL.Emit(OpCodes.Ret);
            }

            // Add untyped parameter constructor: new Generated(object param1, ..., object paramN)
            var untypedConstructorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                generatedFields.Select(_ => typeof(object)).ToArray());

            var untypedConstructorIL = typedConstructorBuilder.GetILGenerator();
            for (int i = 0; i < generatedFields.Length; i++)
            {
                var fieldType = generatedFields[i].FieldType;

                untypedConstructorIL.Emit(OpCodes.Ldarg_0);
                untypedConstructorIL.Emit(OpCodes.Ldarg_1);
                if (!fieldType.IsClass)
                {
                    untypedConstructorIL.Emit(OpCodes.Unbox_Any, fieldType);
                }
                else if (fieldType != typeof(object))
                {
                    untypedConstructorIL.Emit(OpCodes.Castclass, fieldType);
                }
                untypedConstructorIL.Emit(OpCodes.Stfld, generatedFields[i]);
                untypedConstructorIL.Emit(OpCodes.Ret);
            }

            // Add parameterless constructor: new Generated()
            var parameterlessConstructorBuilder = typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

            return new GeneratedConstructors
            {
                Parameterless = parameterlessConstructorBuilder,
                Typed = typedConstructorBuilder,
                Untyped = untypedConstructorBuilder,
            };
        }
    }
}
