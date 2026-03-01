// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using System.CommandLine;
using Metaschema.Tool.Commands;

namespace Metaschema.Tool;

/// <summary>
/// Entry point for the Metaschema CLI tool.
/// </summary>
public static class Program
{
    /// <summary>
    /// Main entry point.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>Exit code.</returns>
    public static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Metaschema CLI tool for validation, schema generation, code generation, and format conversion")
        {
            new ValidateModuleCommand(),
            new ValidateContentCommand(),
            new GenerateSchemaCommand(),
            new GenerateCodeCommand(),
            new ConvertCommand()
        };

        return await rootCommand.Parse(args).InvokeAsync();
    }
}
