using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ServiceFabric.Remoting.Bond.Test
{
    public class BondRemotingSerializationProviderTests
    {
        [Fact]
        public void Tests()
        {
            var provider = new BondRemotingSerializationProvider();
            var messageFactory = provider.CreateMessageBodyFactory();
            Assert.True(messageFactory is BondMessageFactory);

            var requestTypes = new List<Type> { typeof(BondGameInfo), typeof(string), typeof(List<string>) };
            var serializer1 = provider.CreateRequestMessageSerializer(null, requestTypes);
            var serializer2 = provider.CreateRequestMessageSerializer(null, requestTypes);
            Assert.True(serializer1 is BondRequestMessageBodySerializer);
            Assert.Same(serializer1, serializer2);

            var emptySerializer1 = provider.CreateRequestMessageSerializer(null, Enumerable.Empty<Type>());
            var emptySerializer2 = provider.CreateRequestMessageSerializer(null, Enumerable.Empty<Type>());
            Assert.True(emptySerializer1 is BondRequestMessageBodySerializer);
            Assert.Same(emptySerializer1, emptySerializer2);

            var serializer3 = provider.CreateRequestMessageSerializer(null, new[] { typeof(string) });
            Assert.NotEqual(serializer3, serializer1);
            Assert.NotEqual(serializer3, serializer2);
            Assert.NotEqual(serializer3, emptySerializer1);
        }
    }
}
