// Licensed under the MIT License.

using System.CommandLine;
using Metaschema.Core.Constraints;
using Metaschema.Core.Loading;
using Metaschema.Core.Model;
using Metaschema.Databind;
using Metaschema.Databind.Nodes;
using Metaschema.Databind.Validation;

namespace Metaschema.Cli.Commands;

/// <summary>
/// Command to validate content against a Metaschema definition.
/// </summary>
#pragma warning disable CA1010 // Inherited from System.CommandLine.Command
public sealed class ValidateContentCommand : Command
#pragma warning restore CA1010
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateContentCommand"/> class.
    /// </summary>
    public ValidateContentCommand()
        : base("validate-content", "Validate content against a Metaschema definition")
    {
        var fileArgument = new Argument<FileInfo>("content-file")
        {
            Description = "The content file to validate"
        };

        var metaschemaOption = new Option<FileInfo>("--metaschema", "-m")
        {
            Description = "The Metaschema module file that defines the content structure",
            Required = true
        };

        var formatOption = new Option<ContentFormat>("--format", "-f")
        {
            Description = "Content format (xml, json, yaml, or auto to detect)",
            DefaultValueFactory = _ => ContentFormat.Auto
        };

        var outputOption = new Option<OutputFormat>("--output", "-o")
        {
            Description = "Output format for validation results",
            DefaultValueFactory = _ => OutputFormat.Text
        };

        Arguments.Add(fileArgument);
        Options.Add(metaschemaOption);
        Options.Add(formatOption);
        Options.Add(outputOption);

        this.SetAction(async (parseResult, cancellationToken) =>
        {
            var contentFile = parseResult.GetValue(fileArgument)!;
            var metaschemaFile = parseResult.GetValue(metaschemaOption)!;
            var contentFormat = parseResult.GetValue(formatOption);
            var outputFormat = parseResult.GetValue(outputOption);
            return await ExecuteAsync(contentFile, metaschemaFile, contentFormat, outputFormat);
        });
    }

    private static async Task<int> ExecuteAsync(
        FileInfo contentFile,
        FileInfo metaschemaFile,
        ContentFormat contentFormat,
        OutputFormat outputFormat)
    {
        var result = new ContentValidationResult
        {
            ContentFile = contentFile.FullName,
            MetaschemaFile = metaschemaFile.FullName,
            Valid = false,
            Errors = []
        };

        // Check files exist
        if (!contentFile.Exists)
        {
            result.Errors.Add($"Content file not found: {contentFile.FullName}");
            OutputResult(result, outputFormat);
            return 1;
        }

        if (!metaschemaFile.Exists)
        {
            result.Errors.Add($"Metaschema file not found: {metaschemaFile.FullName}");
            OutputResult(result, outputFormat);
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

            // Determine format
            var format = contentFormat == ContentFormat.Auto
                ? DetectFormat(contentFile)
                : MapFormat(contentFormat);

            // Load and validate content
            var boundLoader = bindingContext.NewBoundLoader();
            using var contentStream = contentFile.OpenRead();

            var rootNode = boundLoader.Load(contentStream, format);

            if (rootNode is not null)
            {
                result.Valid = true;
                result.RootElementName = rootNode.Name;

                // Perform constraint validation
                var constraintResults = ValidateConstraints(rootNode, module);
                foreach (var finding in constraintResults.Findings)
                {
                    var prefix = finding.Severity switch
                    {
                        ConstraintLevel.Critical => "[CRITICAL]",
                        ConstraintLevel.Error => "[ERROR]",
                        ConstraintLevel.Warning => "[WARNING]",
                        ConstraintLevel.Informational => "[INFO]",
                        _ => "[INFO]"
                    };
                    result.Findings.Add($"{prefix} {finding.Location}: {finding.Message}");
                }

                result.ConstraintErrorCount = constraintResults.CriticalCount + constraintResults.ErrorCount;
                result.ConstraintWarningCount = constraintResults.WarningCount;

                // Only mark invalid if there are constraint errors (not just warnings)
                if (!constraintResults.IsValid)
                {
                    result.Valid = false;
                }
            }
            else
            {
                result.Errors.Add("Failed to load content: no root element found");
            }
        }
        catch (ModuleLoadException ex)
        {
            result.Errors.Add($"Failed to load Metaschema: {ex.Message}");
        }
        catch (SerializationException ex)
        {
            result.Errors.Add($"Content validation error: {ex.Message}");
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Unexpected error: {ex.Message}");
        }

        OutputResult(result, outputFormat);

        return result.Valid ? 0 : 1;
    }

    private static Format DetectFormat(FileInfo file)
    {
        return file.Extension.ToLowerInvariant() switch
        {
            ".xml" => Format.Xml,
            ".json" => Format.Json,
            ".yaml" or ".yml" => Format.Yaml,
            _ => Format.Xml // Default to XML
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

    private static void OutputResult(ContentValidationResult result, OutputFormat format)
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
                    Console.WriteLine($"Valid: {result.ContentFile}");
                    Console.WriteLine($"  Root element: {result.RootElementName}");
                    if (result.ConstraintWarningCount > 0)
                    {
                        Console.WriteLine($"  Warnings: {result.ConstraintWarningCount}");
                    }
                }
                else
                {
                    Console.WriteLine($"Invalid: {result.ContentFile}");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"  {error}");
                    }
                    if (result.ConstraintErrorCount > 0)
                    {
                        Console.WriteLine($"  Constraint errors: {result.ConstraintErrorCount}");
                    }
                }

                // Show all findings
                foreach (var finding in result.Findings)
                {
                    Console.WriteLine($"  {finding}");
                }
                break;
        }
    }

    private sealed class ContentValidationResult
    {
        public required string ContentFile { get; set; }
        public required string MetaschemaFile { get; set; }
        public bool Valid { get; set; }
        public string? RootElementName { get; set; }
        public List<string> Errors { get; set; } = [];
        public List<string> Findings { get; set; } = [];
        public int ConstraintErrorCount { get; set; }
        public int ConstraintWarningCount { get; set; }
    }

    private static ValidationResults ValidateConstraints(DocumentNode rootNode, MetaschemaModule module)
    {
        // Collect all constraints from the module
        var constraints = new List<IConstraint>();

        foreach (var flag in module.FlagDefinitions)
        {
            constraints.AddRange(flag.Constraints);
        }

        foreach (var field in module.FieldDefinitions)
        {
            constraints.AddRange(field.Constraints);
        }

        foreach (var assembly in module.AssemblyDefinitions)
        {
            constraints.AddRange(assembly.Constraints);
        }

        // If no constraints, return empty results
        if (constraints.Count == 0)
        {
            return ValidationResults.Empty;
        }

        // Adapt the document node to INodeItem for constraint validation
        var nodeItem = DocumentNodeAdapter.Adapt(rootNode);

        // Run constraint validation
        var validator = ConstraintValidator.Create();
        return validator.ValidateAll(nodeItem, constraints);
    }
}
