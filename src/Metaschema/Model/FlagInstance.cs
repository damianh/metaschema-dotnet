// Licensed under the MIT License.

using Metaschema.Markup;

namespace Metaschema.Model;

/// <summary>
/// A flag instance is a reference to a flag definition within a field or assembly.
/// </summary>
public sealed class FlagInstance
{
    /// <summary>
    /// Gets the name of the referenced flag definition.
    /// </summary>
    public required string Ref { get; init; }

    /// <summary>
    /// Gets whether this flag is required.
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>
    /// Gets the override for formal name from the definition.
    /// </summary>
    public string? FormalName { get; init; }

    /// <summary>
    /// Gets the override for description from the definition.
    /// </summary>
    public MarkupLine? Description { get; init; }

    /// <summary>
    /// Gets the override for the effective name.
    /// </summary>
    public string? UseName { get; init; }

    /// <summary>
    /// Gets the effective name for this instance.
    /// </summary>
    public string EffectiveName => UseName ?? ResolvedDefinition?.EffectiveName ?? Ref;

    /// <summary>
    /// Gets additional notes.
    /// </summary>
    public MarkupMultiline? Remarks { get; init; }

    /// <summary>
    /// Gets the version when deprecated.
    /// </summary>
    public string? DeprecatedVersion { get; init; }

    /// <summary>
    /// Gets or sets the resolved flag definition.
    /// </summary>
    public FlagDefinition? ResolvedDefinition { get; set; }
}
