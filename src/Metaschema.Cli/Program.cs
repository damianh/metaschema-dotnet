// Licensed under the MIT License.

namespace Metaschema.Cli;

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
    public static int Main(string[] args)
    {
        // TODO: Implement CLI using System.CommandLine
        // Commands to implement:
        // - validate-module
        // - validate-content
        // - generate-schema
        // - convert
        // - generate-code

        if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
        {
            Console.WriteLine("Metaschema CLI tool for validation, schema generation, and format conversion.");
            Console.WriteLine();
            Console.WriteLine("Usage: metaschema <command> [options]");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  validate-module    Validate a Metaschema module definition");
            Console.WriteLine("  validate-content   Validate content against a Metaschema");
            Console.WriteLine("  generate-schema    Generate XSD or JSON Schema from a Metaschema");
            Console.WriteLine("  convert            Convert content between formats");
            Console.WriteLine("  generate-code      Generate C# code from a Metaschema");
            Console.WriteLine();
            Console.WriteLine("Run 'metaschema <command> --help' for more information on a command.");
            return 0;
        }

        Console.Error.WriteLine($"Unknown command: {args[0]}");
        return 1;
    }
}
