// Licensed under the MIT License.

using Metaschema.Core.Loading;
using Metaschema.Databind;
using Metaschema.Databind.Nodes;

namespace Oscal.Sample.Typed.Examples;

/// <summary>
/// Compares the dynamic API (from Oscal.Sample.Dynamic) with the typed API approach.
/// </summary>
public static class CompareDynamicVsTypedExample
{
    public static void Run()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Example 2: Compare Dynamic vs Typed APIs                    ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // First, load the catalog using the dynamic API
        Console.WriteLine("Loading OSCAL Catalog using Dynamic API...");
        var loader = new ModuleLoader();
        var metaschemaPath = Path.Combine(AppContext.BaseDirectory, "Metaschema", "oscal_catalog_metaschema.xml");
        var module = loader.Load(metaschemaPath);

        var context = new BindingContext();
        context.RegisterModule(module);

        var catalogPath = Path.Combine(AppContext.BaseDirectory, "Content", "catalog", "NIST_SP-800-53_rev5_catalog.json");
        var deserializer = context.GetDeserializer(Format.Json);
        var document = deserializer.Deserialize(File.ReadAllText(catalogPath));

        Console.WriteLine("  Loaded NIST SP 800-53 Rev 5 Catalog");
        Console.WriteLine();

        // Compare Dynamic vs Typed API code
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  COMPARISON: Getting Catalog Title                           ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Dynamic API approach
        Console.WriteLine("Dynamic API:");
        Console.WriteLine("─────────────────────────────────────────────────────────────");
        Console.WriteLine(@"  var metadata = catalog.ModelChildren
      .FirstOrDefault(c => c.Name == ""metadata"") as AssemblyNode;
  var title = metadata?.ModelChildren
      .FirstOrDefault(c => c.Name == ""title"") as FieldNode;
  Console.WriteLine(title?.RawValue);");
        Console.WriteLine();

        // Actually run the dynamic code
        var catalog = document.RootAssembly;
        if (catalog != null)
        {
            var metadata = catalog.ModelChildren.FirstOrDefault(c => c.Name == "metadata") as AssemblyNode;
            var title = metadata?.ModelChildren.FirstOrDefault(c => c.Name == "title") as FieldNode;
            Console.WriteLine($"  Result: {title?.RawValue ?? "(not found)"}");
        }
        Console.WriteLine();

        // Typed API approach
        Console.WriteLine("Typed API (with generated classes):");
        Console.WriteLine("─────────────────────────────────────────────────────────────");
        Console.WriteLine(@"  Console.WriteLine(catalog.Metadata?.Title);");
        Console.WriteLine();
        Console.WriteLine("  Result: (same value, but with compile-time safety)");
        Console.WriteLine();

        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  COMPARISON: Count Controls in Access Control Family         ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Dynamic API approach
        Console.WriteLine("Dynamic API:");
        Console.WriteLine("─────────────────────────────────────────────────────────────");
        Console.WriteLine(@"  var acGroup = catalog.ModelChildren
      .Where(c => c.Name == ""group"")
      .Cast<AssemblyNode>()
      .FirstOrDefault(g => g.Flags.TryGetValue(""id"", out var id) 
                           && id.RawValue == ""ac"");
  var controlCount = acGroup?.ModelChildren
      .Count(c => c.Name == ""control"") ?? 0;");
        Console.WriteLine();

        // Actually run the dynamic code
        if (catalog != null)
        {
            var acGroup = catalog.ModelChildren
                .Where(c => c.Name == "group")
                .Cast<AssemblyNode>()
                .FirstOrDefault(g => g.Flags.TryGetValue("id", out var id) && id.RawValue == "ac");
            var controlCount = acGroup?.ModelChildren.Count(c => c.Name == "control") ?? 0;
            Console.WriteLine($"  Result: {controlCount} controls");
        }
        Console.WriteLine();

        // Typed API approach
        Console.WriteLine("Typed API (with generated classes):");
        Console.WriteLine("─────────────────────────────────────────────────────────────");
        Console.WriteLine(@"  var controlCount = catalog.Groups
      .First(g => g.Id == ""ac"")
      .Controls.Count;");
        Console.WriteLine();
        Console.WriteLine("  Result: (same value, with IntelliSense support)");
        Console.WriteLine();

        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  KEY DIFFERENCES                                             ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        Console.WriteLine("  Feature                    | Dynamic API       | Typed API");
        Console.WriteLine("  ───────────────────────────|───────────────────|──────────────────");
        Console.WriteLine("  Property access            | String-based      | Property access");
        Console.WriteLine("  Compile-time checking      | No                | Yes");
        Console.WriteLine("  IntelliSense support       | Limited           | Full");
        Console.WriteLine("  Refactoring support        | Manual            | Automatic");
        Console.WriteLine("  Type casting               | Required          | Not needed");
        Console.WriteLine("  Runtime flexibility        | High              | Lower");
        Console.WriteLine("  Schema binding             | At runtime        | At compile time");
        Console.WriteLine("  Binary size                | Smaller           | Larger");
        Console.WriteLine();

        Console.WriteLine("Compare dynamic vs typed example complete!");
    }
}
