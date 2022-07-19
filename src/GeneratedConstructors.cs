// --------------------------------------------------------------------------------
// <copyright file="BondResponseMessageBodyTypeGenerator.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace Microsoft.ServiceFabric.Services.Remoting.V2.Bond
{
    using System.Reflection.Emit;

    internal readonly struct GeneratedConstructors
    {
        public ConstructorBuilder Parameterless { get; init; }
        public ConstructorBuilder Typed { get; init; }
        public ConstructorBuilder Untyped { get; init; }
    }
}
