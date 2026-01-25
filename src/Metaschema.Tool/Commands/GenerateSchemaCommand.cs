// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Metaschema.Core.Loading;
using Metaschema.Schemagen;
using Metaschema.Schemagen.JsonSchema;
using Metaschema.Schemagen.Xsd;

namespace Metaschema.Tool.Commands;

/// <summary>
/// Command to generate XSD or JSON Schema from a Metaschema module.
/// </summary>
#pragma warning disable CA1010 // Inherited from System.CommandLine.Command
public sealed class GenerateSchemaCommand : Command
#pragma warning restore CA1010
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GenerateSchemaCommand"/> class.
    /// </summary>
    public GenerateSchemaCommand()
        : base("generate-schema", "Generate XSD or JSON Schema from a Metaschema module")
    {
        var fileArgument = new Argument<FileInfo>("metaschema-file")
        {
            Description = "The Metaschema module file"
        };

        var typeOption = new Option<SchemaType>("--type", "-t")
        {
            Description = "Schema type to generate (xsd or json-schema)",
            Required = true
        };

        var outputOption = new Option<FileInfo?>("--output", "-o")
        {
            Description = "Output file path (defaults to stdout)"
        };

        var inlineOption = new Option<bool>("--inline-definitions")
        {
            Description = "Inline all definitions instead of using references",
            DefaultValueFactory = _ => false
        };

        var noDocsOption = new Option<bool>("--no-documentation")
        {
            Description = "Exclude documentation annotations from schema",
            DefaultValueFactory = _ => false
        };

        Arguments.Add(fileArgument);
        Options.Add(typeOption);
        Options.Add(outputOption);
        Options.Add(inlineOption);
        Options.Add(noDocsOption);

        this.SetAction(async (parseResult, cancellationToken) =>
        {
            var file = parseResult.GetValue(fileArgument)!;
            var schemaType = parseResult.GetValue(typeOption);
            var output = parseResult.GetValue(outputOption);
            var inlineDefinitions = parseResult.GetValue(inlineOption);
            var noDocumentation = parseResult.GetValue(noDocsOption);
            return await ExecuteAsync(file, schemaType, output, inlineDefinitions, noDocumentation);
        });
    }

    private static readonly JsonSerializerOptions JsonWriteOptions = new()
    {
        WriteIndented = true
    };

    private static async Task<int> ExecuteAsync(
        FileInfo file,
        SchemaType schemaType,
        FileInfo? output,
        bool inlineDefinitions,
        bool noDocumentation)
    {
        if (!file.Exists)
        {
            await Console.Error.WriteLineAsync($"Error: File not found: {file.FullName}");
            return 1;
        }

        try
        {
            // Load the Metaschema module
            var loader = new ModuleLoader();
            using var stream = file.OpenRead();
            var uri = new Uri(file.FullName);
            var module = loader.Load(stream, uri);

            // Configure generation options
            var options = new SchemaGenerationOptions
            {
                InlineDefinitions = inlineDefinitions,
                IncludeDocumentation = !noDocumentation
            };

            string schemaContent;

            if (schemaType == SchemaType.Xsd)
            {
                var generator = new XsdGenerator(options);
                var xsd = generator.Generate(module);
                schemaContent = xsd.ToString();
            }
            else
            {
                var generator = new JsonSchemaGenerator(options);
                using var jsonDoc = generator.Generate(module);
                schemaContent = JsonSerializer.Serialize(jsonDoc.RootElement, JsonWriteOptions);
            }

            // Write output
            if (output is not null)
            {
                await File.WriteAllTextAsync(output.FullName, schemaContent);
                Console.WriteLine($"Schema written to: {output.FullName}");
            }
            else
            {
                Console.WriteLine(schemaContent);
            }

            return 0;
        }
        catch (ModuleLoadException ex)
        {
            await Console.Error.WriteLineAsync($"Error loading Metaschema: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error generating schema: {ex.Message}");
            return 1;
        }
    }
}

/// <summary>
/// Schema type to generate.
/// </summary>
public enum SchemaType
{
    /// <summary>
    /// XML Schema (XSD).
    /// </summary>
    Xsd,

    /// <summary>
    /// JSON Schema.
    /// </summary>
    JsonSchema
}
