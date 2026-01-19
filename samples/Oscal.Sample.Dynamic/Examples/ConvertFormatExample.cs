// Licensed under the MIT License.

using Metaschema.Core.Loading;
using Metaschema.Databind;

namespace Oscal.Sample.Dynamic.Examples;

/// <summary>
/// Demonstrates converting OSCAL documents between JSON, XML, and YAML formats.
/// 
/// OSCAL supports three serialization formats: JSON, XML, and YAML. This example
/// shows how to load content in one format and serialize it to another.
/// </summary>
public static class ConvertFormatExample
{
    public static void Run()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Example 6: Convert Between Formats                          ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Step 1: Load the profile metaschema and a profile document
        Console.WriteLine("Step 1: Loading OSCAL Profile...");
        var loader = new ModuleLoader();
        var metaschemaPath = Path.Combine(AppContext.BaseDirectory, "Metaschema", "oscal_profile_metaschema.xml");
        var module = loader.Load(metaschemaPath);

        var context = new BindingContext();
        context.RegisterModule(module);

        // Load the MODERATE baseline profile (smaller than the full catalog)
        var profilePath = Path.Combine(
            AppContext.BaseDirectory,
            "Content",
            "profile",
            "NIST_SP-800-53_rev5_LOW-baseline_profile.json");

        var jsonContent = File.ReadAllText(profilePath);
        var jsonDeserializer = context.GetDeserializer(Format.Json);
        var document = jsonDeserializer.Deserialize(jsonContent);

        Console.WriteLine($"  Loaded: NIST SP 800-53 LOW Baseline Profile (JSON)");
        Console.WriteLine($"  Original size: {jsonContent.Length:N0} characters");
        Console.WriteLine();

        // Step 2: Convert to XML
        Console.WriteLine("Step 2: Converting to XML...");
        var xmlSerializer = context.GetSerializer(Format.Xml);
        var xmlContent = xmlSerializer.SerializeToString(document);

        Console.WriteLine($"  XML size: {xmlContent.Length:N0} characters");
        Console.WriteLine();
        Console.WriteLine("  XML preview (first 500 chars):");
        Console.WriteLine("  " + new string('-', 60));
        var xmlPreview = xmlContent.Length > 500 ? xmlContent[..500] + "..." : xmlContent;
        foreach (var line in xmlPreview.Split('\n').Take(10))
        {
            Console.WriteLine($"  {line.TrimEnd()}");
        }
        Console.WriteLine("  " + new string('-', 60));
        Console.WriteLine();

        // Step 3: Convert to YAML
        Console.WriteLine("Step 3: Converting to YAML...");
        var yamlSerializer = context.GetSerializer(Format.Yaml);
        var yamlContent = yamlSerializer.SerializeToString(document);

        Console.WriteLine($"  YAML size: {yamlContent.Length:N0} characters");
        Console.WriteLine();
        Console.WriteLine("  YAML preview (first 500 chars):");
        Console.WriteLine("  " + new string('-', 60));
        var yamlPreview = yamlContent.Length > 500 ? yamlContent[..500] + "..." : yamlContent;
        foreach (var line in yamlPreview.Split('\n').Take(15))
        {
            Console.WriteLine($"  {line.TrimEnd()}");
        }
        Console.WriteLine("  " + new string('-', 60));
        Console.WriteLine();

        // Step 4: Convert back to JSON (round-trip)
        Console.WriteLine("Step 4: Converting back to JSON (round-trip)...");
        var jsonSerializer = context.GetSerializer(Format.Json);
        var roundTripJson = jsonSerializer.SerializeToString(document);

        Console.WriteLine($"  Round-trip JSON size: {roundTripJson.Length:N0} characters");
        Console.WriteLine();

        // Step 5: Summary
        Console.WriteLine("Step 5: Format Comparison...");
        Console.WriteLine();
        Console.WriteLine("  Format | Size (chars) | Relative Size");
        Console.WriteLine("  -------|--------------|---------------");
        Console.WriteLine($"  JSON   | {jsonContent.Length,12:N0} | 100%");
        Console.WriteLine($"  XML    | {xmlContent.Length,12:N0} | {100.0 * xmlContent.Length / jsonContent.Length:F0}%");
        Console.WriteLine($"  YAML   | {yamlContent.Length,12:N0} | {100.0 * yamlContent.Length / jsonContent.Length:F0}%");
        Console.WriteLine();

        // Step 6: Save converted files (optional - just show paths)
        Console.WriteLine("Step 6: Output File Paths...");
        var outputDir = Path.Combine(AppContext.BaseDirectory, "Output");
        Console.WriteLine($"  To save converted files, outputs would go to:");
        Console.WriteLine($"    XML:  {Path.Combine(outputDir, "profile.xml")}");
        Console.WriteLine($"    YAML: {Path.Combine(outputDir, "profile.yaml")}");
        Console.WriteLine($"    JSON: {Path.Combine(outputDir, "profile.json")}");

        Console.WriteLine();
        Console.WriteLine("Format conversion example complete!");
    }
}
