// Licensed under the MIT License.

using System.Text.Json;
using Metaschema.Core.Loading;
using Metaschema.Schemagen;
using Metaschema.Schemagen.JsonSchema;
using Metaschema.Schemagen.Xsd;

namespace Oscal.Sample.Dynamic.Examples;

/// <summary>
/// Demonstrates generating JSON Schema and XSD from OSCAL Metaschema modules.
/// 
/// Schema generation allows you to:
/// - Validate OSCAL content using standard schema validators
/// - Generate editor auto-completion hints
/// - Create documentation for OSCAL data structures
/// </summary>
public static class GenerateSchemaExample
{
    public static void Run()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Example 7: Generate Schemas from Metaschema                 ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        var loader = new ModuleLoader();

        // Step 1: Generate JSON Schema from Catalog Metaschema
        Console.WriteLine("Step 1: Generating JSON Schema for OSCAL Catalog...");
        var catalogMetaschemaPath = Path.Combine(AppContext.BaseDirectory, "Metaschema", "oscal_catalog_metaschema.xml");
        var catalogModule = loader.Load(catalogMetaschemaPath);

        var jsonSchemaOptions = new SchemaGenerationOptions
        {
            IncludeDocumentation = true
        };
        var jsonSchemaGenerator = new JsonSchemaGenerator(jsonSchemaOptions);
        var catalogJsonSchema = jsonSchemaGenerator.Generate(catalogModule);

        var jsonSchemaText = FormatJsonDocument(catalogJsonSchema);
        Console.WriteLine($"  Generated JSON Schema: {jsonSchemaText.Length:N0} characters");
        Console.WriteLine();
        Console.WriteLine("  JSON Schema preview:");
        Console.WriteLine("  " + new string('-', 60));
        var schemaPreview = jsonSchemaText.Length > 800 ? jsonSchemaText[..800] + "\n  ..." : jsonSchemaText;
        foreach (var line in schemaPreview.Split('\n').Take(20))
        {
            Console.WriteLine($"  {line}");
        }
        Console.WriteLine("  " + new string('-', 60));
        Console.WriteLine();

        // Step 2: Generate XSD from Catalog Metaschema
        Console.WriteLine("Step 2: Generating XSD for OSCAL Catalog...");
        var xsdGenerator = new XsdGenerator();
        var catalogXsd = xsdGenerator.Generate(catalogModule);

        var catalogXsdText = catalogXsd.ToString();
        Console.WriteLine($"  Generated XSD: {catalogXsdText.Length:N0} characters");
        Console.WriteLine();
        Console.WriteLine("  XSD preview:");
        Console.WriteLine("  " + new string('-', 60));
        var xsdText = catalogXsdText;
        var xsdPreview = xsdText.Length > 800 ? xsdText[..800] + "\n  ..." : xsdText;
        foreach (var line in xsdPreview.Split('\n').Take(15))
        {
            Console.WriteLine($"  {line.TrimEnd()}");
        }
        Console.WriteLine("  " + new string('-', 60));
        Console.WriteLine();

        // Step 3: Generate schemas for other OSCAL models
        Console.WriteLine("Step 3: Generating Schemas for All OSCAL Models...");
        Console.WriteLine();

        var models = new[]
        {
            ("oscal_catalog_metaschema.xml", "Catalog"),
            ("oscal_profile_metaschema.xml", "Profile"),
            ("oscal_ssp_metaschema.xml", "SSP"),
            ("oscal_component_metaschema.xml", "Component Definition"),
            ("oscal_assessment-plan_metaschema.xml", "Assessment Plan"),
            ("oscal_assessment-results_metaschema.xml", "Assessment Results"),
            ("oscal_poam_metaschema.xml", "POA&M")
        };

        Console.WriteLine("  Model                  | JSON Schema | XSD");
        Console.WriteLine("  -----------------------|-------------|--------");

        foreach (var (filename, displayName) in models)
        {
            var metaschemaPath = Path.Combine(AppContext.BaseDirectory, "Metaschema", filename);

            if (!File.Exists(metaschemaPath))
            {
                Console.WriteLine($"  {displayName,-22} | (not found) | (not found)");
                continue;
            }

            try
            {
                var module = loader.Load(metaschemaPath);

                var jsonSchema = jsonSchemaGenerator.Generate(module);
                var jsonSchemaSize = FormatJsonDocument(jsonSchema).Length;

                var xsd = xsdGenerator.Generate(module);
                var xsdSize = xsd.ToString().Length;

                Console.WriteLine($"  {displayName,-22} | {jsonSchemaSize / 1024,7:N0} KB | {xsdSize / 1024,4:N0} KB");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  {displayName,-22} | Error: {ex.Message}");
            }
        }
        Console.WriteLine();

        // Step 4: Output paths
        Console.WriteLine("Step 4: Output File Paths...");
        var outputDir = Path.Combine(AppContext.BaseDirectory, "Output", "schemas");
        Console.WriteLine($"  To save generated schemas, outputs would go to:");
        Console.WriteLine($"    JSON Schema: {Path.Combine(outputDir, "oscal-catalog.json")}");
        Console.WriteLine($"    XSD:         {Path.Combine(outputDir, "oscal-catalog.xsd")}");

        Console.WriteLine();
        Console.WriteLine("Schema generation example complete!");
    }

    private static string FormatJsonDocument(JsonDocument doc)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
        doc.WriteTo(writer);
        writer.Flush();
        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }
}
