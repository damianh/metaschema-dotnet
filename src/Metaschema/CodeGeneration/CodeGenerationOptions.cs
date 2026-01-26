// Licensed under the MIT License.

namespace Metaschema.CodeGeneration;

/// <summary>
/// Options for C# code generation from Metaschema modules.
/// </summary>
public sealed class CodeGenerationOptions
{
    /// <summary>
    /// Gets or sets the target namespace for generated code.
    /// </summary>
    public string Namespace { get; set; } = "Generated";

    /// <summary>
    /// Gets or sets the output directory path.
    /// </summary>
    public string? OutputPath { get; set; }

    /// <summary>
    /// Gets or sets the visibility of generated types.
    /// </summary>
    public TypeVisibility Visibility { get; set; } = TypeVisibility.Public;

    /// <summary>
    /// Gets or sets whether to generate nullable reference type annotations.
    /// </summary>
    public bool NullableAnnotations { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to generate records instead of classes.
    /// </summary>
    public bool UseRecords { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to generate System.Text.Json serialization context.
    /// </summary>
    public bool GenerateJsonContext { get; set; } = true;

    /// <summary>
    /// Gets or sets the name of the generated JsonSerializerContext class.
    /// </summary>
    public string? JsonContextName { get; set; }

    /// <summary>
    /// Gets or sets whether to generate extension methods for load/save operations.
    /// </summary>
    public bool GenerateExtensionMethods { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to generate a separate file per type.
    /// </summary>
    public bool FilePerType { get; set; }

    /// <summary>
    /// Gets or sets whether to include XML documentation comments.
    /// </summary>
    public bool IncludeDocumentation { get; set; } = true;

    /// <summary>
    /// Gets or sets a prefix for generated class names.
    /// </summary>
    public string? ClassPrefix { get; set; }

    /// <summary>
    /// Gets or sets a suffix for generated class names.
    /// </summary>
    public string? ClassSuffix { get; set; }

    /// <summary>
    /// Gets or sets whether to use file-scoped namespaces.
    /// </summary>
    public bool FileScopedNamespaces { get; set; } = true;
}

/// <summary>
/// Visibility level for generated types.
/// </summary>
public enum TypeVisibility
{
    /// <summary>
    /// Types are public.
    /// </summary>
    Public,

    /// <summary>
    /// Types are internal.
    /// </summary>
    Internal
}
