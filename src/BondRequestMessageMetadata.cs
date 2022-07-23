using System;
using System.Collections.Generic;

namespace ServiceFabric.Remoting.Bond
{
    [global::Bond.Schema]
    internal class BondRequestMessageMetadata
    {
        [global::Bond.Id(0), global::Bond.Required]
        public string MethodName { get; init; }

        [global::Bond.Id(1), global::Bond.Required]
        public List<string> ParameterTypeNames { get; init; }

        [global::Bond.Id(2), global::Bond.Required]
        public ArraySegment<byte> Payload { get; init; }

        public BondRequestMessageMetadata()
        {
            this.MethodName = string.Empty;
            this.ParameterTypeNames = new List<string>();
        }
    }
}
