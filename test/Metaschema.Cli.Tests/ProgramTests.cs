// Licensed under the MIT License.

using Shouldly;
using Xunit;

namespace Metaschema.Cli.Tests;

public class ProgramTests
{
    [Fact]
    public void Main_WithNoArgs_ShouldReturnZero()
    {
        var exitCode = Program.Main([]);
        exitCode.ShouldBe(0);
    }
}
