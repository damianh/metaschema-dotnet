// Licensed under the MIT License.

using System.Diagnostics;

namespace Oscal.Sample.Typed.Examples;

/// <summary>
/// Demonstrates using the CLI tool to generate C# code from Metaschema modules.
/// </summary>
public static class CliCodeGenExample
{
    public static void Run()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Example 3: CLI Code Generation Demo                         ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        Console.WriteLine("The metaschema CLI tool can generate C# code from Metaschema modules.");
        Console.WriteLine();

        Console.WriteLine("Step 1: CLI Generate Command...");
        Console.WriteLine("─────────────────────────────────────────────────────────────");
        Console.WriteLine(@"
  metaschema generate <metaschema-file> --output <directory> [options]

  Options:
    --namespace <ns>     Namespace for generated types (default: Generated)
    --visibility <vis>   Type visibility: public or internal (default: public)

  Example:
    metaschema generate oscal_catalog_metaschema.xml \
        --output ./Generated \
        --namespace Oscal.Catalog
");

        Console.WriteLine("Step 2: Generated File Structure...");
        Console.WriteLine("─────────────────────────────────────────────────────────────");
        Console.WriteLine(@"
  Generated/
  ├── Catalog.cs           # Root document type
  ├── Metadata.cs          # Metadata assembly type
  ├── Group.cs             # Control group type
  ├── Control.cs           # Security control type
  ├── Part.cs              # Control part type
  ├── Parameter.cs         # Parameter type
  ├── Property.cs          # Property type
  ├── Link.cs              # Link type
  └── Types/               # Flag/primitive types
      ├── Uuid.cs          # UUID wrapper struct
      ├── Token.cs         # Token wrapper struct
      └── ...
");

        Console.WriteLine("Step 3: Example Generated Code...");
        Console.WriteLine("─────────────────────────────────────────────────────────────");
        Console.WriteLine(@"
  // Catalog.cs (generated)
  namespace Oscal.Catalog;

  /// <summary>
  /// A structured, organized collection of control information.
  /// </summary>
  public partial class Catalog
  {
      /// <summary>
      /// Uniquely identifies this catalog.
      /// </summary>
      public Guid Uuid { get; set; }

      /// <summary>
      /// Provides information about the publication and availability.
      /// </summary>
      public Metadata? Metadata { get; set; }

      /// <summary>
      /// A group of controls, or of groups of controls.
      /// </summary>
      public List<Group> Groups { get; set; } = [];

      /// <summary>
      /// A collection of resources for this catalog.
      /// </summary>
      public BackMatter? BackMatter { get; set; }
  }
");

        Console.WriteLine("Step 4: Trying the CLI Tool...");
        Console.WriteLine("─────────────────────────────────────────────────────────────");

        var metaschemaPath = Path.Combine(AppContext.BaseDirectory, "Metaschema", "oscal_catalog_metaschema.xml");

        if (!File.Exists(metaschemaPath))
        {
            Console.WriteLine($"  Metaschema file not found: {metaschemaPath}");
            Console.WriteLine("  Skipping CLI demonstration.");
        }
        else
        {
            Console.WriteLine($"  Would generate from: {metaschemaPath}");
            Console.WriteLine();

            // Check if the CLI tool is available
            var cliPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", 
                "src", "Metaschema.Cli", "bin", "Debug", "net10.0", "metaschema.exe");
            
            if (File.Exists(cliPath))
            {
                Console.WriteLine("  CLI tool found. Running 'metaschema --help':");
                Console.WriteLine();

                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = cliPath,
                        Arguments = "--help",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var process = Process.Start(psi);
                    if (process != null)
                    {
                        var output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit(5000);
                        
                        foreach (var line in output.Split('\n').Take(20))
                        {
                            Console.WriteLine($"  {line}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Error running CLI: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("  CLI tool not found at expected path.");
                Console.WriteLine("  Build the CLI project first with:");
                Console.WriteLine("    dotnet build src/Metaschema.Cli");
            }
        }

        Console.WriteLine();
        Console.WriteLine("CLI code generation demo complete!");
    }
}
