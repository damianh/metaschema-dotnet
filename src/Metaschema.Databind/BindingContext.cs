// Licensed under the MIT License.

using Metaschema.Core.Model;
using Metaschema.Databind.Nodes;
using Metaschema.Databind.Serialization;

namespace Metaschema.Databind;

/// <summary>
/// Default implementation of <see cref="IBindingContext"/>.
/// </summary>
public sealed class BindingContext : IBindingContext
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
    public BindingContext(MetaschemaModule module) : this()
    {
        RegisterModule(module);
    }

    /// <inheritdoc />
    public IEnumerable<MetaschemaModule> Modules => _modules;

    /// <inheritdoc />
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

    /// <inheritdoc />
    public ISerializer GetSerializer(Format format)
    {
        return format switch
        {
            Format.Xml => new XmlContentSerializer(this),
            Format.Json => new JsonContentSerializer(this),
            Format.Yaml => new YamlContentSerializer(this),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unknown format")
        };
    }

    /// <inheritdoc />
    public IDeserializer GetDeserializer(Format format)
    {
        return format switch
        {
            Format.Xml => new XmlContentDeserializer(this),
            Format.Json => new JsonContentDeserializer(this),
            Format.Yaml => new YamlContentDeserializer(this),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unknown format")
        };
    }

    /// <inheritdoc />
    public IBoundLoader NewBoundLoader()
    {
        return new BoundLoader(this);
    }

    /// <inheritdoc />
    public AssemblyDefinition? ResolveRootAssembly(string rootName)
    {
        return _rootAssembliesByName.GetValueOrDefault(rootName);
    }

    /// <inheritdoc />
    public AssemblyDefinition? ResolveRootAssembly(string rootName, Uri namespaceUri)
    {
        return _rootAssembliesByNameAndNamespace.GetValueOrDefault((rootName, namespaceUri));
    }
}
