// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using Metaschema.Constraints;
using Metaschema.Markup;

namespace Metaschema.Model;

/// <summary>
/// A flag definition represents a simple named value (leaf node).
/// Flags are like XML attributes - they have no child elements.
/// </summary>
public sealed class FlagDefinition
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
    /// Gets the default value when the flag is omitted.
    /// </summary>
    public string? DefaultValue { get; init; }

    /// <summary>
    /// Gets the constraints defined on this flag.
    /// </summary>
    public IReadOnlyList<IConstraint> Constraints { get; init; } = [];
}
