// Licensed under the MIT License.

using Shouldly;
using Xunit;

namespace Metaschema.Cli.Tests;

public class ProgramTests
{
    [Fact]
    public async Task Main_WithNoArgs_ShouldReturnError()
    {
        // When no command is provided, System.CommandLine shows usage and returns error
        var exitCode = await Program.Main([]);
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task Main_WithHelpFlag_ShouldReturnZero()
    {
        var exitCode = await Program.Main(["--help"]);
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task Main_WithVersionFlag_ShouldReturnZero()
    {
        var exitCode = await Program.Main(["--version"]);
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task Main_WithInvalidCommand_ShouldReturnNonZero()
    {
        var exitCode = await Program.Main(["invalid-command"]);
        exitCode.ShouldNotBe(0);
    }

    [Fact]
    public async Task ValidateModule_WithMissingFile_ShouldReturnError()
    {
        var exitCode = await Program.Main(["validate-module", "non-existent-file.xml"]);
        exitCode.ShouldBe(1);
    }
}
