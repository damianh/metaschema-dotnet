// Licensed under the MIT License.

using System.CommandLine;
using Metaschema.Core.Loading;
using Metaschema.Databind.CodeGeneration;

namespace Metaschema.Cli.Commands;

/// <summary>
/// Command to generate C# code from a Metaschema module.
/// </summary>
#pragma warning disable CA1010 // Inherited from System.CommandLine.Command
public sealed class GenerateCodeCommand : Command
#pragma warning restore CA1010
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GenerateCodeCommand"/> class.
    /// </summary>
    public GenerateCodeCommand()
        : base("generate-code", "Generate C# code from a Metaschema module")
    {
        var fileArgument = new Argument<FileInfo>("metaschema-file")
        {
            Description = "The Metaschema module file"
        };

        var namespaceOption = new Option<string>("--namespace", "-n")
        {
            Description = "Target namespace for generated code",
            DefaultValueFactory = _ => "Generated"
        };

        var outputOption = new Option<DirectoryInfo?>("--output", "-o")
        {
            Description = "Output directory (defaults to current directory)"
        };

        var visibilityOption = new Option<TypeVisibility>("--visibility", "-v")
        {
            Description = "Visibility of generated types (public or internal)",
            DefaultValueFactory = _ => TypeVisibility.Public
        };

        var filePerTypeOption = new Option<bool>("--file-per-type")
        {
            Description = "Generate a separate file for each type",
            DefaultValueFactory = _ => false
        };

        var prefixOption = new Option<string?>("--prefix")
        {
            Description = "Prefix for generated class names"
        };

        var suffixOption = new Option<string?>("--suffix")
        {
            Description = "Suffix for generated class names"
        };

        var noDocsOption = new Option<bool>("--no-documentation")
        {
            Description = "Exclude XML documentation comments",
            DefaultValueFactory = _ => false
        };

        var noNullableOption = new Option<bool>("--no-nullable")
        {
            Description = "Disable nullable reference type annotations",
            DefaultValueFactory = _ => false
        };

        Arguments.Add(fileArgument);
        Options.Add(namespaceOption);
        Options.Add(outputOption);
        Options.Add(visibilityOption);
        Options.Add(filePerTypeOption);
        Options.Add(prefixOption);
        Options.Add(suffixOption);
        Options.Add(noDocsOption);
        Options.Add(noNullableOption);

        this.SetAction(async (parseResult, cancellationToken) =>
        {
            var file = parseResult.GetValue(fileArgument)!;
            var ns = parseResult.GetValue(namespaceOption)!;
            var output = parseResult.GetValue(outputOption);
            var visibility = parseResult.GetValue(visibilityOption);
            var filePerType = parseResult.GetValue(filePerTypeOption);
            var prefix = parseResult.GetValue(prefixOption);
            var suffix = parseResult.GetValue(suffixOption);
            var noDocs = parseResult.GetValue(noDocsOption);
            var noNullable = parseResult.GetValue(noNullableOption);

            return await ExecuteAsync(file, ns, output, visibility, filePerType, prefix, suffix, noDocs, noNullable);
        });
    }

    private static async Task<int> ExecuteAsync(
        FileInfo file,
        string ns,
        DirectoryInfo? output,
        TypeVisibility visibility,
        bool filePerType,
        string? prefix,
        string? suffix,
        bool noDocs,
        bool noNullable)
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
            var options = new CodeGenerationOptions
            {
                Namespace = ns,
                OutputPath = output?.FullName,
                Visibility = visibility,
                FilePerType = filePerType,
                ClassPrefix = prefix,
                ClassSuffix = suffix,
                IncludeDocumentation = !noDocs,
                NullableAnnotations = !noNullable
            };

            // Generate code
            var generator = new CSharpCodeGenerator(options);
            var files = generator.Generate(module);

            // Determine output directory
            var outputDir = output?.FullName ?? Environment.CurrentDirectory;
            Directory.CreateDirectory(outputDir);

            // Write files
            foreach (var (fileName, content) in files)
            {
                var filePath = Path.Combine(outputDir, fileName);
                await File.WriteAllTextAsync(filePath, content);
                Console.WriteLine($"Generated: {filePath}");
            }

            Console.WriteLine($"Code generation complete. {files.Count} file(s) generated.");
            return 0;
        }
        catch (ModuleLoadException ex)
        {
            await Console.Error.WriteLineAsync($"Error loading Metaschema: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error generating code: {ex.Message}");
            return 1;
        }
    }
}
