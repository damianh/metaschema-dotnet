// Licensed under the MIT License.
// Demonstrates using the CLI tool to generate C# code from Metaschema modules.

using System.Diagnostics;

Console.WriteLine("The metaschema CLI tool can generate C# code from Metaschema modules.");
Console.WriteLine();

Console.WriteLine("CLI Generate Command:");
Console.WriteLine("  metaschema generate <metaschema-file> --output <directory> [options]");
Console.WriteLine();
Console.WriteLine("  Options:");
Console.WriteLine("    --namespace <ns>     Namespace for generated types (default: Generated)");
Console.WriteLine("    --visibility <vis>   Type visibility: public or internal (default: public)");
Console.WriteLine();
Console.WriteLine("  Example:");
Console.WriteLine("    metaschema generate oscal_catalog_metaschema.xml \\");
Console.WriteLine("        --output ./Generated \\");
Console.WriteLine("        --namespace Oscal.Catalog");
Console.WriteLine();

Console.WriteLine("Generated File Structure:");
Console.WriteLine("  Generated/");
Console.WriteLine("  ├── Catalog.cs           # Root document type");
Console.WriteLine("  ├── Metadata.cs          # Metadata assembly type");
Console.WriteLine("  ├── Group.cs             # Control group type");
Console.WriteLine("  ├── Control.cs           # Security control type");
Console.WriteLine("  ├── Part.cs              # Control part type");
Console.WriteLine("  ├── Parameter.cs         # Parameter type");
Console.WriteLine("  ├── Property.cs          # Property type");
Console.WriteLine("  ├── Link.cs              # Link type");
Console.WriteLine("  └── Types/               # Flag/primitive types");
Console.WriteLine();

Console.WriteLine("Example Generated Code:");
Console.WriteLine(@"
  // Catalog.cs (generated)
  namespace Oscal.Catalog;

  /// <summary>
  /// A structured, organized collection of control information.
  /// </summary>
  public partial class Catalog
  {
      public Guid Uuid { get; set; }
      public Metadata? Metadata { get; set; }
      public List<Group> Groups { get; set; } = [];
      public BackMatter? BackMatter { get; set; }
  }
");

// Try to run the CLI tool if available
var metaschemaPath = Path.Combine(AppContext.BaseDirectory, "Metaschema", "oscal_catalog_metaschema.xml");

if (File.Exists(metaschemaPath))
{
    Console.WriteLine($"Metaschema file available: {metaschemaPath}");
    Console.WriteLine();

    var cliPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..",
        "src", "Metaschema.Cli", "bin", "Debug", "net10.0", "metaschema.exe");

    if (File.Exists(cliPath))
    {
        Console.WriteLine("CLI tool found. Running 'metaschema --help':");
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
                    Console.WriteLine($"  {line}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Error running CLI: {ex.Message}");
        }
    }
    else
    {
        Console.WriteLine("CLI tool not found. Build it first with:");
        Console.WriteLine("  dotnet build src/Metaschema.Cli");
    }
}
