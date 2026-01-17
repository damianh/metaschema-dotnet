// Licensed under the MIT License.

using Metaschema.Core.Markup;

namespace Metaschema.Core.Model;

/// <summary>
/// A field instance is a reference to a field definition within an assembly model.
/// </summary>
public sealed class FieldInstance : ModelElement
{
    /// <summary>
    /// Gets the name of the referenced field definition.
    /// </summary>
    public required string Ref { get; init; }

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
    /// Gets the minimum occurrences (default: 0).
    /// </summary>
    public int MinOccurs { get; init; }

    /// <summary>
    /// Gets the maximum occurrences (null = unbounded, default: 1).
    /// </summary>
    public int? MaxOccurs { get; init; } = 1;

    /// <summary>
    /// Gets the grouping configuration for collections.
    /// </summary>
    public GroupAs? GroupAs { get; init; }

    /// <summary>
    /// Gets the XML wrapping behavior.
    /// </summary>
    public XmlWrapping InXml { get; init; } = XmlWrapping.Wrapped;

    /// <summary>
    /// Gets or sets the resolved field definition.
    /// </summary>
    public FieldDefinition? ResolvedDefinition { get; set; }
}
