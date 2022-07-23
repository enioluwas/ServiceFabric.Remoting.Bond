using System;

namespace ServiceFabric.Remoting.Bond
{
    [global::Bond.Schema]
    internal class BondResponseMessageMetadata
    {
        [global::Bond.Id(0), global::Bond.Required]
        public string MethodName { get; init; }

        [global::Bond.Id(1), global::Bond.Required]
        public string ReturnTypeName { get; init; }

        [global::Bond.Id(2), global::Bond.Required]
        public ArraySegment<byte> Payload { get; init; }

        public BondResponseMessageMetadata()
        {
            this.MethodName = string.Empty;
            this.ReturnTypeName = string.Empty;
        }
    }
}
