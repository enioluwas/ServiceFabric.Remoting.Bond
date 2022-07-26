using Microsoft.ServiceFabric.Services.Remoting.V2;
using Xunit;

namespace ServiceFabric.Remoting.Bond.Test
{
    public class BondResponseMessageBodyTypeGeneratorTests
    {
        [Fact]
        public void Generate_Type_GeneratesType()
        {
            var type = typeof(string);
            var generator = new BondResponseMessageBodyTypeGenerator(null);
            var generatedType = generator.Generate(type);
            var innerResponse = "Test";
            var response = new BondResponseMessageBody("TestInterface", "TestMethod");
            response.Set(innerResponse);
            var generatedTypeInstance = generatedType.InstanceFactory(response) as IServiceRemotingResponseMessageBody;
            Assert.NotNull(generatedTypeInstance);
            Assert.Equal(innerResponse, generatedTypeInstance.Get(typeof(void)));
        }
    }
}
