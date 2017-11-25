using System;
using System.Text;
using Bond;
using Bond.IO.Unsafe;
using Bond.Protocols;
using FluentAssertions;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using SceneSkope.ServiceFabric.Remoting.Bond;
using Xunit;

namespace TestRemoting
{
    public class TestTypeBuilder
    {
        [Fact]
        public void BasicCreateTest()
        {
            var type = BondMessageTypeBuilder.CreateObject(new[] { typeof(int), typeof(string) });
            var constructor = type.GetConstructors()[1];
            var constructed = constructor.Invoke(new object[] { 1, "two" });
            Assert.NotNull(constructed);
        }

        [Fact]
        public void BasicCreateWithObjectConstructorTest()
        {
            var type = BondMessageTypeBuilder.CreateObject(new[] { typeof(int), typeof(string) });
            var constructor = type.GetConstructors()[2];
            var constructed = constructor.Invoke(new object[] { 1, "two" });
            Assert.NotNull(constructed);
        }

        [Fact]
        public void BasicBondSerialisationTest()
        {
            var type = BondMessageTypeBuilder.CreateObject(new[] { typeof(int), typeof(string) });
            var constructor = type.GetConstructors()[1];
            var constructed = constructor.Invoke(new object[] { 1, "two" });

            var serializer = new Serializer<FastBinaryWriter<OutputBuffer>>(type);
            var output = new OutputBuffer();
            var writer = new FastBinaryWriter<OutputBuffer>(output);
            serializer.Serialize(constructed, writer);

            var deserializer = new Deserializer<FastBinaryReader<InputBuffer>>(type);
            var input = new InputBuffer(output.Data);
            var reader = new FastBinaryReader<InputBuffer>(input);
            var deserialized = deserializer.Deserialize(reader);

            deserialized.ShouldBeEquivalentTo(constructed);
        }

        [Fact]
        public void ArraySegmentBondSerialisationTest()
        {
            var type = BondMessageTypeBuilder.CreateObject(new[] { typeof(string), typeof(ArraySegment<byte>) });
            var segment = new ArraySegment<byte>(Encoding.UTF8.GetBytes("this is a test"));
            var constructor = type.GetConstructors()[1];
            var constructed = constructor.Invoke(new object[] { "machine", segment });

            var serializer = new Serializer<FastBinaryWriter<OutputBuffer>>(type);
            var output = new OutputBuffer();
            var writer = new FastBinaryWriter<OutputBuffer>(output);
            serializer.Serialize(constructed, writer);

            var deserializer = new Deserializer<FastBinaryReader<InputBuffer>>(type);
            var input = new InputBuffer(output.Data);
            var reader = new FastBinaryReader<InputBuffer>(input);
            var deserialized = deserializer.Deserialize(reader);

            deserialized.ShouldBeEquivalentTo(constructed);
        }

        [Fact]
        public void DateTimeBondSerialisationTest()
        {
            var type = BondMessageTypeBuilder.CreateObject(new[] { typeof(string), typeof(DateTime) });
            var timestamp = DateTime.UtcNow;
            var constructor = type.GetConstructors()[1];
            var constructed = constructor.Invoke(new object[] { "machine", timestamp });

            var serializer = new Serializer<FastBinaryWriter<OutputBuffer>>(type);
            var output = new OutputBuffer();
            var writer = new FastBinaryWriter<OutputBuffer>(output);
            serializer.Serialize(constructed, writer);

            var deserializer = new Deserializer<FastBinaryReader<InputBuffer>>(type);
            var input = new InputBuffer(output.Data);
            var reader = new FastBinaryReader<InputBuffer>(input);
            var deserialized = deserializer.Deserialize(reader);

            deserialized.ShouldBeEquivalentTo(constructed);
        }

        [Fact]
        public void GetBondTypeAliasConverter()
        {
            var type = BondMessageTypeBuilder.CreateObject(new[] { typeof(string), typeof(Guid) });
            var name = type.AssemblyQualifiedName;
            var converterName = type.Namespace + ".BondTypeAliasConverter" + name.Substring(type.FullName.Length);
            var converter = Type.GetType(converterName);
            Assert.NotNull(converter);
        }

        [Fact]
        public void GuidBondSerialisationTest()
        {
            var type = BondMessageTypeBuilder.CreateObject(new[] { typeof(string), typeof(Guid) });
            var id = Guid.NewGuid();
            var constructor = type.GetConstructors()[1];
            var constructed = constructor.Invoke(new object[] { "machine", id });

            var serializer = new Serializer<FastBinaryWriter<OutputBuffer>>(type);
            var deserializer = new Deserializer<FastBinaryReader<InputBuffer>>(type);
            var output = new OutputBuffer();
            var writer = new FastBinaryWriter<OutputBuffer>(output);
            serializer.Serialize(constructed, writer);

            var input = new InputBuffer(output.Data);
            var reader = new FastBinaryReader<InputBuffer>(input);
            var deserialized = deserializer.Deserialize(reader);

            deserialized.ShouldBeEquivalentTo(constructed);
        }

        [Fact]
        public void Int64BondSerialisationTest()
        {
            var type = BondMessageTypeBuilder.CreateObject(new[] { typeof(string), typeof(long) });
            const long longValue = 100L;
            var constructor = type.GetConstructors()[1];
            var constructed = constructor.Invoke(new object[] { "machine", longValue });

            var serializer = new Serializer<FastBinaryWriter<OutputBuffer>>(type);
            var output = new OutputBuffer();
            var writer = new FastBinaryWriter<OutputBuffer>(output);
            serializer.Serialize(constructed, writer);

            var deserializer = new Deserializer<FastBinaryReader<InputBuffer>>(type);
            var input = new InputBuffer(output.Data);
            var reader = new FastBinaryReader<InputBuffer>(input);
            var deserialized = deserializer.Deserialize(reader);

            deserialized.ShouldBeEquivalentTo(constructed);
        }

        [Fact]
        public void ArraySegmentBondSerialisationTestForRequestBody()
        {
            var type = BondMessageTypeBuilder.CreateRequestMessageBody(new[] { typeof(string), typeof(ArraySegment<byte>) });
            var segment = new ArraySegment<byte>(Encoding.UTF8.GetBytes("this is a test"));
            var constructor = type.GetConstructors()[1];
            var constructed = constructor.Invoke(new object[] { "machine", segment });

            var serializer = new Serializer<FastBinaryWriter<OutputBuffer>>(type);
            var output = new OutputBuffer();
            var writer = new FastBinaryWriter<OutputBuffer>(output);
            serializer.Serialize(constructed, writer);

            var deserializer = new Deserializer<FastBinaryReader<InputBuffer>>(type);
            var input = new InputBuffer(output.Data);
            var reader = new FastBinaryReader<InputBuffer>(input);
            var deserialized = deserializer.Deserialize(reader);

            deserialized.ShouldBeEquivalentTo(constructed);
        }

        [Theory]
        [InlineData(typeof(int), 1)]
        [InlineData(typeof(string), "test")]
        public void BasicResponseMessageTest(Type valueType, object value)
        {
            var createdType = BondMessageTypeBuilder.CreateResponseMessageBody(valueType);
            var constructed = createdType.GetConstructors()[1].Invoke(new object[] { value }) as IServiceRemotingResponseMessageBody;
            Assert.NotNull(constructed);
        }

        [Theory]
        [InlineData(typeof(int), 1, 10)]
        [InlineData(typeof(string), "test", "testset")]
        public void FullResponseMessageTest(Type valueType, object value, object setValue)
        {
            var createdType = BondMessageTypeBuilder.CreateResponseMessageBody(valueType);
            var constructed = createdType.GetConstructors()[1].Invoke(new object[] { value }) as IServiceRemotingResponseMessageBody;

            var gotValue = constructed.Get(valueType);
            value.ShouldBeEquivalentTo(value);

            constructed.Set(setValue);
            constructed.Get(valueType).ShouldBeEquivalentTo(setValue);
        }

        [Theory]
        [InlineData(typeof(int), 1)]
        [InlineData(typeof(string), "test")]
        public void BasicRequestMessageTest(Type valueType, object value)
        {
            var createdType = BondMessageTypeBuilder.CreateRequestMessageBody(new[] { typeof(int), valueType, typeof(string) });
            var constructed = createdType.GetConstructors()[1].Invoke(new object[] { 0, value, "other" }) as IServiceRemotingRequestMessageBody;
            Assert.NotNull(constructed);
        }

        [Theory]
        [InlineData(typeof(int), 1, 20)]
        [InlineData(typeof(string), "test", "testset")]
        public void FullRequestMessageTest(Type valueType, object value, object setValue)
        {
            var createdType = BondMessageTypeBuilder.CreateRequestMessageBody(new[] { typeof(int), valueType, typeof(string) });
            var constructed = createdType.GetConstructors()[1].Invoke(new object[] { 0, value, "other" }) as IServiceRemotingRequestMessageBody;
            Assert.NotNull(constructed);

            var gotValue = constructed.GetParameter(1, "second", valueType);
            value.ShouldBeEquivalentTo(value);

            constructed.SetParameter(1, "second", setValue);
            constructed.GetParameter(1, "second", valueType).ShouldBeEquivalentTo(setValue);
        }

        [Theory]
        [InlineData(0, "invalid string", typeof(InvalidCastException))]
        [InlineData(1, 1, typeof(InvalidCastException))]
        [InlineData(2, "invalid parameter", typeof(ArgumentOutOfRangeException))]
        public void InvalidRequestMessageTest(int parameterNumber, object nextValue, Type exceptionType)
        {
            var createdType = BondMessageTypeBuilder.CreateRequestMessageBody(new[] { typeof(int), typeof(string) });
            var constructed = createdType.GetConstructors()[1].Invoke(new object[] { 0, "other" }) as IServiceRemotingRequestMessageBody;
            Assert.NotNull(constructed);

            if (exceptionType != typeof(InvalidCastException))
            {
                Assert.Throws(exceptionType, () => constructed.GetParameter(parameterNumber, "test", typeof(object)));
            }
            Assert.Throws(exceptionType, () => constructed.SetParameter(parameterNumber, "test", nextValue));
        }
    }
}
