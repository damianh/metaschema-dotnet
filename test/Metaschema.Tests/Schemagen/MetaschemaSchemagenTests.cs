// Licensed under the MIT License.

using Shouldly;
using Xunit;

namespace Metaschema.Schemagen;

public class MetaschemaSchemagenTests
{
    [Fact]
    public void Version_ShouldReturnValue()
    {
        var version = MetaschemaSchemagen.Version;
        version.ShouldNotBeNullOrEmpty();
    }
}
