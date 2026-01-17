// Licensed under the MIT License.

using Metaschema.Core.Markup;

namespace Metaschema.Core.Model;

/// <summary>
/// A field definition represents a value container with optional flags (edge node).
/// Fields have a value and can have flag children, but no field/assembly children.
/// </summary>
public sealed class FieldDefinition
{
    /// <summary>
    /// Gets the unique identifier within the module.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the override for the effective name in data instances.
    /// </summary>
    public string? UseName { get; init; }

    /// <summary>
    /// Gets the effective name (UseName if set, otherwise Name).
    /// </summary>
    public string EffectiveName => UseName ?? Name;

    /// <summary>
    /// Gets the human-readable label for documentation.
    /// </summary>
    public string? FormalName { get; init; }

    /// <summary>
    /// Gets the semantic description of the definition.
    /// </summary>
    public MarkupLine? Description { get; init; }

    /// <summary>
    /// Gets the visibility scope (global or local).
    /// </summary>
    public Scope Scope { get; init; } = Scope.Global;

    /// <summary>
    /// Gets the version when this definition was deprecated.
    /// </summary>
    public string? DeprecatedVersion { get; init; }

    /// <summary>
    /// Gets additional notes and clarifications.
    /// </summary>
    public MarkupMultiline? Remarks { get; init; }

    /// <summary>
    /// Gets the module containing this definition.
    /// </summary>
    public required MetaschemaModule ContainingModule { get; init; }

    /// <summary>
    /// Gets the data type name (default: "string").
    /// </summary>
    public string DataTypeName { get; init; } = "string";

    /// <summary>
    /// Gets the default value when the field is omitted.
    /// </summary>
    public string? DefaultValue { get; init; }

    /// <summary>
    /// Gets whether the field can be collapsed in JSON/YAML.
    /// </summary>
    public bool IsCollapsible { get; init; }

    /// <summary>
    /// Gets the property name for the field value in JSON.
    /// </summary>
    public string? JsonValueKeyName { get; init; }

    /// <summary>
    /// Gets the flag reference for JSON object keys in collections.
    /// </summary>
    public string? JsonKeyFlagRef { get; init; }

    /// <summary>
    /// Gets the flag instances declared on this field.
    /// </summary>
    public IReadOnlyList<FlagInstance> FlagInstances { get; init; } = [];
}
