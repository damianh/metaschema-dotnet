// Licensed under the MIT License.

using System.CommandLine;
using Metaschema.Core.Loading;
using Metaschema.Databind;

namespace Metaschema.Tool.Commands;

/// <summary>
/// Command to convert content between XML, JSON, and YAML formats.
/// </summary>
#pragma warning disable CA1010 // Inherited from System.CommandLine.Command
public sealed class ConvertCommand : Command
#pragma warning restore CA1010
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertCommand"/> class.
    /// </summary>
    public ConvertCommand()
        : base("convert", "Convert content between XML, JSON, and YAML formats")
    {
        var fileArgument = new Argument<FileInfo>("input-file")
        {
            Description = "The input file to convert"
        };

        var metaschemaOption = new Option<FileInfo>("--metaschema", "-m")
        {
            Description = "The Metaschema module file that defines the content structure",
            Required = true
        };

        var toOption = new Option<ContentFormat>("--to", "-t")
        {
            Description = "Target format (xml, json, yaml)",
            Required = true
        };

        var outputOption = new Option<FileInfo?>("--output", "-o")
        {
            Description = "Output file path (defaults to stdout)"
        };

        Arguments.Add(fileArgument);
        Options.Add(metaschemaOption);
        Options.Add(toOption);
        Options.Add(outputOption);

        this.SetAction(async (parseResult, cancellationToken) =>
        {
            var inputFile = parseResult.GetValue(fileArgument)!;
            var metaschemaFile = parseResult.GetValue(metaschemaOption)!;
            var targetFormat = parseResult.GetValue(toOption);
            var outputFile = parseResult.GetValue(outputOption);
            return await ExecuteAsync(inputFile, metaschemaFile, targetFormat, outputFile);
        });
    }

    private static async Task<int> ExecuteAsync(
        FileInfo inputFile,
        FileInfo metaschemaFile,
        ContentFormat targetFormat,
        FileInfo? outputFile)
    {
        if (!inputFile.Exists)
        {
            await Console.Error.WriteLineAsync($"Error: Input file not found: {inputFile.FullName}");
            return 1;
        }

        if (!metaschemaFile.Exists)
        {
            await Console.Error.WriteLineAsync($"Error: Metaschema file not found: {metaschemaFile.FullName}");
            return 1;
        }

        if (targetFormat == ContentFormat.Auto)
        {
            await Console.Error.WriteLineAsync("Error: Target format must be specified (xml, json, or yaml)");
            return 1;
        }

        try
        {
            // Load the Metaschema module
            var loader = new ModuleLoader();
            using var moduleStream = metaschemaFile.OpenRead();
            var moduleUri = new Uri(metaschemaFile.FullName);
            var module = loader.Load(moduleStream, moduleUri);

            // Create binding context and register the module
            var bindingContext = new BindingContext();
            bindingContext.RegisterModule(module);

            // Detect source format
            var sourceFormat = DetectFormat(inputFile);
            var targetDatabindFormat = MapFormat(targetFormat);

            // Load content
            var boundLoader = bindingContext.NewBoundLoader();
            using var inputStream = inputFile.OpenRead();
            var rootNode = boundLoader.Load(inputStream, sourceFormat);

            if (rootNode is null)
            {
                await Console.Error.WriteLineAsync("Error: Failed to load content");
                return 1;
            }

            // Get the appropriate serializer and convert
            var serializer = bindingContext.GetSerializer(targetDatabindFormat);

            using var memoryStream = new MemoryStream();
            serializer.Serialize(rootNode, memoryStream);
            memoryStream.Position = 0;

            using var reader = new StreamReader(memoryStream);
            var content = await reader.ReadToEndAsync();

            // Write output
            if (outputFile is not null)
            {
                await File.WriteAllTextAsync(outputFile.FullName, content);
                Console.WriteLine($"Converted to {targetFormat}: {outputFile.FullName}");
            }
            else
            {
                Console.WriteLine(content);
            }

            return 0;
        }
        catch (ModuleLoadException ex)
        {
            await Console.Error.WriteLineAsync($"Error loading Metaschema: {ex.Message}");
            return 1;
        }
        catch (SerializationException ex)
        {
            await Console.Error.WriteLineAsync($"Error during conversion: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return 1;
        }
    }

    private static Format DetectFormat(FileInfo file)
    {
        return file.Extension.ToLowerInvariant() switch
        {
            ".xml" => Format.Xml,
            ".json" => Format.Json,
            ".yaml" or ".yml" => Format.Yaml,
            _ => Format.Xml
        };
    }

    private static Format MapFormat(ContentFormat format)
    {
        return format switch
        {
            ContentFormat.Xml => Format.Xml,
            ContentFormat.Json => Format.Json,
            ContentFormat.Yaml => Format.Yaml,
            _ => Format.Xml
        };
    }
}
