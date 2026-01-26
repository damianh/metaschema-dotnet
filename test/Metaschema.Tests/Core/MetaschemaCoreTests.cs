// Licensed under the MIT License.

using Shouldly;
using Xunit;

namespace Metaschema;

public class MetaschemaCoreTests
{
    [Fact]
    public void Version_ShouldReturnValue()
    {
        var version = MetaschemaCore.Version;
        version.ShouldNotBeNullOrEmpty();
    }
}
