// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using Shouldly;
using Xunit;

namespace Metaschema.SchemaGeneration;

public class MetaschemaSchemagenTests
{
    [Fact]
    public void Version_ShouldReturnValue()
    {
        var version = MetaschemaSchemagen.Version;
        version.ShouldNotBeNullOrEmpty();
    }
}
