// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using System.CommandLine;
using Metaschema.CodeGeneration;
using Metaschema.Loading;

namespace Metaschema.Tool.Commands;

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

        var useRecordsOption = new Option<bool>("--use-records")
        {
            Description = "Generate records instead of classes (recommended)",
            DefaultValueFactory = _ => true
        };

        var jsonContextOption = new Option<string?>("--json-context")
        {
            Description = "Name for the JsonSerializerContext class (only with --use-records)"
        };

        var noExtensionsOption = new Option<bool>("--no-extensions")
        {
            Description = "Don't generate extension methods",
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
        Options.Add(useRecordsOption);
        Options.Add(jsonContextOption);
        Options.Add(noExtensionsOption);

        SetAction(async (parseResult, cancellationToken) =>
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
            var useRecords = parseResult.GetValue(useRecordsOption);
            var jsonContext = parseResult.GetValue(jsonContextOption);
            var noExtensions = parseResult.GetValue(noExtensionsOption);

            return await ExecuteAsync(file, ns, output, visibility, filePerType, prefix, suffix, noDocs, noNullable, useRecords, jsonContext, noExtensions);
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
        bool noNullable,
        bool useRecords,
        string? jsonContext,
        bool noExtensions)
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
                NullableAnnotations = !noNullable,
                UseRecords = useRecords,
                GenerateJsonContext = useRecords,
                JsonContextName = jsonContext,
                GenerateExtensionMethods = !noExtensions
            };

            // Generate code using appropriate generator
            Dictionary<string, string> files;
            if (useRecords)
            {
                Console.WriteLine("Generating C# records with System.Text.Json support...");
                var generator = new RecordCodeGenerator(options);
                files = generator.Generate(module);
            }
            else
            {
                Console.WriteLine("Generating C# classes...");
                var generator = new CSharpCodeGenerator(options);
                files = generator.Generate(module);
            }

            // Determine output directory
            var outputDir = output?.FullName ?? Environment.CurrentDirectory;
            Directory.CreateDirectory(outputDir);

            // Write files
            foreach (var (fileName, content) in files)
            {
                var filePath = Path.Combine(outputDir, fileName);
                await File.WriteAllTextAsync(filePath, content);
                Console.WriteLine($"  Generated: {fileName}");
            }

            Console.WriteLine();
            Console.WriteLine($"✓ Code generation complete. {files.Count} file(s) generated.");

            if (useRecords)
            {
                Console.WriteLine();
                Console.WriteLine("Next steps:");
                Console.WriteLine("  1. Add generated files to your .csproj:");
                Console.WriteLine($"     <Compile Include=\"{Path.GetFileName(outputDir)}\\**\\*.g.cs\" />");
                Console.WriteLine("  2. Build your project - the STJ source generator will complete the JsonContext");
                Console.WriteLine("  3. Use the generated types:");

                var rootAssembly = module.AssemblyDefinitions.FirstOrDefault(a => a.RootName is not null);
                if (rootAssembly is not null)
                {
                    var typeName = ToPascalCase(rootAssembly.Name);
                    Console.WriteLine($"     var data = Extensions.LoadFromJson(\"file.json\");");
                    Console.WriteLine($"     Console.WriteLine(data.{GetFirstProperty(rootAssembly)});");
                }
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
            await Console.Error.WriteLineAsync($"Error generating code: {ex.Message}");
            await Console.Error.WriteLineAsync(ex.StackTrace);
            return 1;
        }
    }

    private static string ToPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        var parts = name.Split('-', '_', '.');
        return string.Concat(parts.Select(p =>
            p.Length > 0 ? char.ToUpperInvariant(p[0]) + p[1..] : p));
    }

    private static string GetFirstProperty(Metaschema.Model.AssemblyDefinition assembly)
    {
        var firstField = assembly.Model?.Elements.OfType<Metaschema.Model.FieldInstance>().FirstOrDefault();
        var firstAssembly = assembly.Model?.Elements.OfType<Metaschema.Model.AssemblyInstance>().FirstOrDefault();

        if (firstField is not null)
        {
            return ToPascalCase(firstField.EffectiveName);
        }
        if (firstAssembly is not null)
        {
            return ToPascalCase(firstAssembly.EffectiveName);
        }
        return "Property";
    }
}
