// Licensed under the MIT License.

using Shouldly;
using Xunit;

namespace Metaschema.Databind.Tests;

public class MetaschemaDatabindTests
{
    [Fact]
    public void Version_ShouldReturnValue()
    {
        var version = MetaschemaDatabind.Version;
        version.ShouldNotBeNullOrEmpty();
    }
}
