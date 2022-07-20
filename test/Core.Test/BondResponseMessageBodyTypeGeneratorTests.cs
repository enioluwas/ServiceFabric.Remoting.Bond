using Microsoft.ServiceFabric.Services.Remoting.V2;
using Xunit;

namespace ServiceFabric.Bond.Remoting.Test
{
    public class BondResponseMessageBodyTypeGeneratorTests
    {
        [Fact]
        public void Generate_Type_GeneratesType()
        {
            var type = typeof(string);
            var generatedType = BondResponseMessageBodyTypeGenerator.Instance.Generate(type);
            var innerResponse = "Test";
            var response = new BondResponseMessageBody();
            response.Set(innerResponse);
            var generatedTypeInstance = generatedType.InstanceFactory(response) as IServiceRemotingResponseMessageBody;
            Assert.NotNull(generatedTypeInstance);
            Assert.Equal(innerResponse, generatedTypeInstance.Get(typeof(void)));
        }
    }
}
