// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using Metaschema.Model;
using Metaschema.Serialization;

namespace Metaschema;

/// <summary>
/// Provides the context for binding Metaschema modules to serialization operations.
/// </summary>
public sealed class BindingContext
{
    private readonly List<MetaschemaModule> _modules = [];
    private readonly Dictionary<string, AssemblyDefinition> _rootAssembliesByName = new(StringComparer.Ordinal);
    private readonly Dictionary<(string Name, Uri Namespace), AssemblyDefinition> _rootAssembliesByNameAndNamespace = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="BindingContext"/> class.
    /// </summary>
    public BindingContext()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BindingContext"/> class with the specified module.
    /// </summary>
    /// <param name="module">The module to register.</param>
    public BindingContext(MetaschemaModule module) : this() => RegisterModule(module);

    /// <summary>
    /// Gets the registered modules.
    /// </summary>
    public IEnumerable<MetaschemaModule> Modules => _modules;

    /// <summary>
    /// Registers a module with this binding context.
    /// </summary>
    /// <param name="metaschemaModule">The module to register.</param>
    public void RegisterModule(MetaschemaModule metaschemaModule)
    {
        ArgumentNullException.ThrowIfNull(metaschemaModule);

        _modules.Add(metaschemaModule);

        // Index root assemblies for quick lookup
        foreach (var assembly in metaschemaModule.RootAssemblyDefinitions)
        {
            if (assembly.RootName is not null)
            {
                _rootAssembliesByName[assembly.RootName] = assembly;
                _rootAssembliesByNameAndNamespace[(assembly.RootName, metaschemaModule.XmlNamespace)] = assembly;
            }
        }

        // Also register imported modules recursively
        foreach (var imported in metaschemaModule.ImportedModules)
        {
            RegisterImportedModule(imported);
        }
    }

    private void RegisterImportedModule(MetaschemaModule module)
    {
        foreach (var assembly in module.RootAssemblyDefinitions)
        {
            if (assembly.RootName is not null)
            {
                // Don't overwrite if already registered (first registration wins for imports)
                _rootAssembliesByName.TryAdd(assembly.RootName, assembly);
                _rootAssembliesByNameAndNamespace.TryAdd((assembly.RootName, module.XmlNamespace), assembly);
            }
        }

        foreach (var imported in module.ImportedModules)
        {
            RegisterImportedModule(imported);
        }
    }

    /// <summary>
    /// Gets a serializer for the specified format.
    /// </summary>
    /// <param name="format">The serialization format.</param>
    /// <returns>A serializer for the format.</returns>
    public ISerializer GetSerializer(Format format) => format switch
    {
        Format.Xml => new XmlContentSerializer(this),
        Format.Json => new JsonContentSerializer(this),
        Format.Yaml => new YamlContentSerializer(this),
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unknown format")
    };

    /// <summary>
    /// Gets a deserializer for the specified format.
    /// </summary>
    /// <param name="format">The serialization format.</param>
    /// <returns>A deserializer for the format.</returns>
    public IDeserializer GetDeserializer(Format format) => format switch
    {
        Format.Xml => new XmlContentDeserializer(this),
        Format.Json => new JsonContentDeserializer(this),
        Format.Yaml => new YamlContentDeserializer(this),
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unknown format")
    };

    /// <summary>
    /// Creates a new bound loader for loading content with format detection.
    /// </summary>
    /// <returns>A new bound loader.</returns>
    public BoundLoader NewBoundLoader() => new BoundLoader(this);

    /// <summary>
    /// Resolves an assembly definition by root name across all registered modules.
    /// </summary>
    /// <param name="rootName">The root element name.</param>
    /// <returns>The assembly definition, or null if not found.</returns>
    public AssemblyDefinition? ResolveRootAssembly(string rootName) => _rootAssembliesByName.GetValueOrDefault(rootName);

    /// <summary>
    /// Resolves an assembly definition by root name and namespace across all registered modules.
    /// </summary>
    /// <param name="rootName">The root element name.</param>
    /// <param name="namespaceUri">The XML namespace URI.</param>
    /// <returns>The assembly definition, or null if not found.</returns>
    public AssemblyDefinition? ResolveRootAssembly(string rootName, Uri namespaceUri) => _rootAssembliesByNameAndNamespace.GetValueOrDefault((rootName, namespaceUri));
}
