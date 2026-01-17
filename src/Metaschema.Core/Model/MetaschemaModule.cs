// Licensed under the MIT License.

using Metaschema.Core.Markup;

namespace Metaschema.Core.Model;

/// <summary>
/// Represents a loaded Metaschema module with all its definitions and imports.
/// </summary>
public sealed class MetaschemaModule
{
    private readonly Dictionary<string, FlagDefinition> _flagDefinitions = new(StringComparer.Ordinal);
    private readonly Dictionary<string, FieldDefinition> _fieldDefinitions = new(StringComparer.Ordinal);
    private readonly Dictionary<string, AssemblyDefinition> _assemblyDefinitions = new(StringComparer.Ordinal);
    private readonly List<MetaschemaModule> _importedModules = [];

    /// <summary>
    /// Gets the human-readable name for the model (schema-name).
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the unique identifier for the module series (short-name).
    /// </summary>
    public required string ShortName { get; init; }

    /// <summary>
    /// Gets the semantic version of the module (schema-version).
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    /// Gets the XML namespace URI for data instances (namespace).
    /// </summary>
    public required Uri XmlNamespace { get; init; }

    /// <summary>
    /// Gets the base URI for JSON Schema $schema keyword (json-base-uri).
    /// </summary>
    public required Uri JsonBaseUri { get; init; }

    /// <summary>
    /// Gets additional documentation about the module.
    /// </summary>
    public MarkupMultiline? Remarks { get; init; }

    /// <summary>
    /// Gets the source location of the module file.
    /// </summary>
    public required Uri Location { get; init; }

    /// <summary>
    /// Gets the modules imported by this module.
    /// </summary>
    public IReadOnlyList<MetaschemaModule> ImportedModules => _importedModules;

    /// <summary>
    /// Gets the flag definitions declared in this module (not imported).
    /// </summary>
    public IEnumerable<FlagDefinition> FlagDefinitions => _flagDefinitions.Values;

    /// <summary>
    /// Gets the field definitions declared in this module (not imported).
    /// </summary>
    public IEnumerable<FieldDefinition> FieldDefinitions => _fieldDefinitions.Values;

    /// <summary>
    /// Gets the assembly definitions declared in this module (not imported).
    /// </summary>
    public IEnumerable<AssemblyDefinition> AssemblyDefinitions => _assemblyDefinitions.Values;

    /// <summary>
    /// Gets definitions exported by this module (scope=global only).
    /// </summary>
    public IEnumerable<FlagDefinition> ExportedFlagDefinitions =>
        FlagDefinitions.Where(d => d.Scope == Scope.Global);

    /// <summary>
    /// Gets definitions exported by this module (scope=global only).
    /// </summary>
    public IEnumerable<FieldDefinition> ExportedFieldDefinitions =>
        FieldDefinitions.Where(d => d.Scope == Scope.Global);

    /// <summary>
    /// Gets definitions exported by this module (scope=global only).
    /// </summary>
    public IEnumerable<AssemblyDefinition> ExportedAssemblyDefinitions =>
        AssemblyDefinitions.Where(d => d.Scope == Scope.Global);

    /// <summary>
    /// Gets assembly definitions that can be document roots (have root-name).
    /// </summary>
    public IEnumerable<AssemblyDefinition> RootAssemblyDefinitions =>
        AssemblyDefinitions.Where(a => a.IsRoot);

    /// <summary>
    /// Resolves a flag definition by name, checking local definitions first,
    /// then imported modules (later imports shadow earlier ones).
    /// </summary>
    /// <param name="name">The definition name to resolve.</param>
    /// <returns>The resolved definition, or null if not found.</returns>
    public FlagDefinition? GetFlagDefinition(string name)
    {
        // Local definitions first (shadow imports)
        if (_flagDefinitions.TryGetValue(name, out var local))
        {
            return local;
        }

        // Imports in reverse order (later imports shadow earlier)
        foreach (var import in _importedModules.AsEnumerable().Reverse())
        {
            var exported = import.ExportedFlagDefinitions.FirstOrDefault(f => f.Name == name);
            if (exported is not null)
            {
                return exported;
            }
        }

        return null;
    }

    /// <summary>
    /// Resolves a field definition by name, checking local definitions first,
    /// then imported modules (later imports shadow earlier ones).
    /// </summary>
    /// <param name="name">The definition name to resolve.</param>
    /// <returns>The resolved definition, or null if not found.</returns>
    public FieldDefinition? GetFieldDefinition(string name)
    {
        // Local definitions first (shadow imports)
        if (_fieldDefinitions.TryGetValue(name, out var local))
        {
            return local;
        }

        // Imports in reverse order (later imports shadow earlier)
        foreach (var import in _importedModules.AsEnumerable().Reverse())
        {
            var exported = import.ExportedFieldDefinitions.FirstOrDefault(f => f.Name == name);
            if (exported is not null)
            {
                return exported;
            }
        }

        return null;
    }

    /// <summary>
    /// Resolves an assembly definition by name, checking local definitions first,
    /// then imported modules (later imports shadow earlier ones).
    /// </summary>
    /// <param name="name">The definition name to resolve.</param>
    /// <returns>The resolved definition, or null if not found.</returns>
    public AssemblyDefinition? GetAssemblyDefinition(string name)
    {
        // Local definitions first (shadow imports)
        if (_assemblyDefinitions.TryGetValue(name, out var local))
        {
            return local;
        }

        // Imports in reverse order (later imports shadow earlier)
        foreach (var import in _importedModules.AsEnumerable().Reverse())
        {
            var exported = import.ExportedAssemblyDefinitions.FirstOrDefault(f => f.Name == name);
            if (exported is not null)
            {
                return exported;
            }
        }

        return null;
    }

    /// <summary>
    /// Adds a flag definition to this module.
    /// </summary>
    /// <param name="definition">The flag definition to add.</param>
    public void AddFlagDefinition(FlagDefinition definition) =>
        _flagDefinitions[definition.Name] = definition;

    /// <summary>
    /// Adds a field definition to this module.
    /// </summary>
    /// <param name="definition">The field definition to add.</param>
    public void AddFieldDefinition(FieldDefinition definition) =>
        _fieldDefinitions[definition.Name] = definition;

    /// <summary>
    /// Adds an assembly definition to this module.
    /// </summary>
    /// <param name="definition">The assembly definition to add.</param>
    public void AddAssemblyDefinition(AssemblyDefinition definition) =>
        _assemblyDefinitions[definition.Name] = definition;

    /// <summary>
    /// Adds an imported module to this module.
    /// </summary>
    /// <param name="module">The imported module to add.</param>
    public void AddImportedModule(MetaschemaModule module) =>
        _importedModules.Add(module);
}
