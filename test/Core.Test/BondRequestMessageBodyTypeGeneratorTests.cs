using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Bond;
using Xunit;

namespace Core.Test
{
    public class BondRequestMessageBodyTypeGeneratorTests
    {
        [Fact]
        public void Generate_Type_GeneratesType()
        {
            var type = typeof(string);
            var generatedType = BondRequestMessageBodyTypeGenerator.Instance.Generate(type);
            var innerResponse = "Test";
            var response = new BondResponseMessageBody();
            response.Set(innerResponse);
            var generatedTypeInstance = generatedType.InstanceFactory(response) as IServiceRemotingResponseMessageBody;
            Assert.NotNull(generatedTypeInstance);
            Assert.Equal(innerResponse, generatedTypeInstance.Get(typeof(void)));
        }
    }
}
