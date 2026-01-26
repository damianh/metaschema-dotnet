// Licensed under the MIT License.

using System.CommandLine;
using Metaschema.Loading;

namespace Metaschema.Tool.Commands;

/// <summary>
/// Command to validate a Metaschema module definition.
/// </summary>
#pragma warning disable CA1010 // Inherited from System.CommandLine.Command
public sealed class ValidateModuleCommand : Command
#pragma warning restore CA1010
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateModuleCommand"/> class.
    /// </summary>
    public ValidateModuleCommand()
        : base("validate-module", "Validate a Metaschema module definition for correctness")
    {
        var fileArgument = new Argument<FileInfo>("metaschema-file")
        {
            Description = "The Metaschema module file to validate"
        };

        var outputOption = new Option<OutputFormat>("--output", "-o")
        {
            Description = "Output format for validation results",
            DefaultValueFactory = _ => OutputFormat.Text
        };

        Arguments.Add(fileArgument);
        Options.Add(outputOption);

        this.SetAction(async (parseResult, cancellationToken) =>
        {
            var file = parseResult.GetValue(fileArgument)!;
            var outputFormat = parseResult.GetValue(outputOption);
            return await ExecuteAsync(file, outputFormat);
        });
    }

    private static async Task<int> ExecuteAsync(FileInfo file, OutputFormat outputFormat)
    {
        if (!file.Exists)
        {
            await Console.Error.WriteLineAsync($"Error: File not found: {file.FullName}");
            return 1;
        }

        var result = new ValidationResult
        {
            File = file.FullName,
            Valid = false,
            Errors = []
        };

        try
        {
            using var stream = file.OpenRead();
            var loader = new ModuleLoader();
            var uri = new Uri(file.FullName);
            var module = loader.Load(stream, uri);

            result.Valid = true;
            result.ModuleName = module.Name;
            result.ModuleVersion = module.Version;

            // Count definitions (use ToList to get efficient Count for IEnumerable)
            var flagDefs = module.FlagDefinitions.ToList();
            var fieldDefs = module.FieldDefinitions.ToList();
            var assemblyDefs = module.AssemblyDefinitions.ToList();
            
            result.Stats = new ModuleStats
            {
                FlagDefinitions = flagDefs.Count,
                FieldDefinitions = fieldDefs.Count,
                AssemblyDefinitions = assemblyDefs.Count,
                ImportedModules = module.ImportedModules.Count
            };
        }
        catch (ModuleLoadException ex)
        {
            result.Errors.Add(ex.Message);
            if (ex.InnerException is not null)
            {
                result.Errors.Add($"  {ex.InnerException.Message}");
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Unexpected error: {ex.Message}");
        }

        OutputResult(result, outputFormat);

        return result.Valid ? 0 : 1;
    }

    private static void OutputResult(ValidationResult result, OutputFormat format)
    {
        switch (format)
        {
            case OutputFormat.Json:
                JsonOutput.Write(result);
                break;

            case OutputFormat.Text:
            default:
                if (result.Valid)
                {
                    Console.WriteLine($"Valid: {result.File}");
                    Console.WriteLine($"  Module: {result.ModuleName} v{result.ModuleVersion}");
                    if (result.Stats is not null)
                    {
                        Console.WriteLine($"  Flags: {result.Stats.FlagDefinitions}");
                        Console.WriteLine($"  Fields: {result.Stats.FieldDefinitions}");
                        Console.WriteLine($"  Assemblies: {result.Stats.AssemblyDefinitions}");
                        Console.WriteLine($"  Imports: {result.Stats.ImportedModules}");
                    }
                }
                else
                {
                    Console.WriteLine($"Invalid: {result.File}");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"  {error}");
                    }
                }
                break;
        }
    }

    private sealed class ValidationResult
    {
        public required string File { get; set; }
        public bool Valid { get; set; }
        public string? ModuleName { get; set; }
        public string? ModuleVersion { get; set; }
        public List<string> Errors { get; set; } = [];
        public ModuleStats? Stats { get; set; }
    }

    private sealed class ModuleStats
    {
        public int FlagDefinitions { get; set; }
        public int FieldDefinitions { get; set; }
        public int AssemblyDefinitions { get; set; }
        public int ImportedModules { get; set; }
    }
}
