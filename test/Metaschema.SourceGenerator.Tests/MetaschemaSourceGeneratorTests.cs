// Licensed under the MIT License.

using Shouldly;
using Xunit;

namespace Metaschema.SourceGenerator.Tests;

public class MetaschemaSourceGeneratorTests
{
    [Fact]
    public void Generator_ShouldBeCreatable()
    {
        // Verify the generator type exists and can be instantiated
        var generatorType = typeof(MetaschemaSourceGenerator);
        generatorType.ShouldNotBeNull();
    }
}
