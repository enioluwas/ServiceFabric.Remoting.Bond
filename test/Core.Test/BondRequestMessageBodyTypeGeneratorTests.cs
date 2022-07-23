using Microsoft.ServiceFabric.Services.Remoting.V2;
using Xunit;

namespace ServiceFabric.Remoting.Bond.Test
{
    public class BondRequestMessageBodyTypeGeneratorTests
    {
        [Fact]
        public void Generate_Type_GeneratesType()
        {
            var types = new[] { typeof(string), typeof(int), typeof(CollectionBehavior) };
            var generatedType = BondRequestMessageBodyTypeGenerator.Instance.Generate(types);
            var innerResponse = new object[] { "Test", 125, CollectionBehavior.CollectionPerClass };
            var response = new BondRequestMessageBody("TestInterface", "TestMethod", innerResponse.Length);
            for (int i = 0; i < innerResponse.Length; i++)
            {
                response.SetParameter(i, "", innerResponse[i]);
            }

            var generatedTypeInstance = generatedType.InstanceFactory(response) as IServiceRemotingRequestMessageBody;
            Assert.NotNull(generatedTypeInstance);
            for (int i = 0; i < innerResponse.Length; i++)
            {
                Assert.Equal(innerResponse[i], generatedTypeInstance.GetParameter(i, "", typeof(void)));
            }
        }
    }
}
