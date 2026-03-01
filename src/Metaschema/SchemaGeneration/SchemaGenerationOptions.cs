// Licensed under the MIT License.

namespace Metaschema.SchemaGeneration;

/// <summary>
/// Options for controlling schema generation behavior.
/// </summary>
public sealed record SchemaGenerationOptions
{
    /// <summary>
    /// Gets whether to inline all definitions instead of using references.
    /// Default is false (use references for reusable definitions).
    /// </summary>
    public bool InlineDefinitions { get; init; }

    /// <summary>
    /// Gets whether to include documentation annotations in the generated schema.
    /// Default is true.
    /// </summary>
    public bool IncludeDocumentation { get; init; } = true;

    /// <summary>
    /// Gets the base URI for the generated schema.
    /// For JSON Schema, this becomes the $id.
    /// If not specified, uses the module's JsonBaseUri.
    /// </summary>
    public Uri? BaseUri { get; init; }

    /// <summary>
    /// Gets the JSON Schema dialect version.
    /// Default is "https://json-schema.org/draft/2020-12/schema".
    /// </summary>
    public string JsonSchemaDialect { get; init; } = "https://json-schema.org/draft/2020-12/schema";

    /// <summary>
    /// Gets the default options instance.
    /// </summary>
    public static SchemaGenerationOptions Default { get; } = new();
}
