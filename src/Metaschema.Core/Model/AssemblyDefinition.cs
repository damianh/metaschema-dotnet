// Licensed under the MIT License.

using Metaschema.Core.Constraints;
using Metaschema.Core.Markup;

namespace Metaschema.Core.Model;

/// <summary>
/// An assembly definition represents a complex composite object (compositional node).
/// Assemblies have no value of their own but contain flags and a model of child elements.
/// </summary>
public sealed class AssemblyDefinition
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
    /// Gets the name when used as a document root element.
    /// </summary>
    public string? RootName { get; init; }

    /// <summary>
    /// Gets whether this assembly can be a document root.
    /// </summary>
    public bool IsRoot => RootName is not null;

    /// <summary>
    /// Gets the flag reference for JSON object keys in collections.
    /// </summary>
    public string? JsonKeyFlagRef { get; init; }

    /// <summary>
    /// Gets the flag instances declared on this assembly.
    /// </summary>
    public IReadOnlyList<FlagInstance> FlagInstances { get; init; } = [];

    /// <summary>
    /// Gets the model containing child field and assembly instances.
    /// </summary>
    public ModelContainer? Model { get; init; }

    /// <summary>
    /// Gets the constraints defined on this assembly.
    /// </summary>
    public IReadOnlyList<IConstraint> Constraints { get; init; } = [];
}
