# Phase 1 Design: Module Loading

## Overview

Phase 1 establishes the core model interfaces and the ability to load Metaschema modules from XML files with import resolution.

## Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              Module Loading                                  │
└─────────────────────────────────────────────────────────────────────────────┘

                         ┌──────────────────────┐
                         │  Metaschema XML File │
                         │  (with <import>s)    │
                         └──────────┬───────────┘
                                    │
                                    ▼
                         ┌──────────────────────┐
                         │   IModuleLoader      │  ← Entry point
                         │   (caches by URI)    │
                         └──────────┬───────────┘
                                    │
              ┌─────────────────────┼─────────────────────┐
              │                     │                     │
              ▼                     ▼                     ▼
    ┌─────────────────┐   ┌─────────────────┐   ┌─────────────────┐
    │IResourceResolver│   │ XmlModuleParser │   │ Cycle Detection │
    │(file, embedded) │   │  (System.Xml)   │   │                 │
    └─────────────────┘   └─────────────────┘   └─────────────────┘
                                    │
                                    ▼
                         ┌──────────────────────┐
                         │       IModule        │  ← In-memory model
                         └──────────┬───────────┘
                                    │
         ┌──────────────────────────┼──────────────────────────┐
         │                          │                          │
         ▼                          ▼                          ▼
┌─────────────────┐      ┌─────────────────┐      ┌─────────────────┐
│ IFlagDefinition │      │ IFieldDefinition│      │IAssemblyDefinition│
└─────────────────┘      └─────────────────┘      └─────────────────┘
```

## Project Structure

```
src/Metaschema.Core/
├── Model/
│   ├── IModule.cs                    # Module interface
│   ├── IDefinition.cs                # Base definition interface
│   ├── IFlagDefinition.cs            # Flag definition
│   ├── IFieldDefinition.cs           # Field definition
│   ├── IAssemblyDefinition.cs        # Assembly definition
│   ├── IFlagInstance.cs              # Flag instance (reference)
│   ├── IFieldInstance.cs             # Field instance (reference)
│   ├── IAssemblyInstance.cs          # Assembly instance (reference)
│   ├── IModelContainer.cs            # Model container for assemblies
│   ├── INamedModelElement.cs         # Common named element interface
│   ├── Scope.cs                      # Enum: Global, Local
│   └── Impl/
│       ├── Module.cs                 # IModule implementation
│       ├── FlagDefinition.cs         
│       ├── FieldDefinition.cs        
│       ├── AssemblyDefinition.cs     
│       ├── FlagInstance.cs           
│       ├── FieldInstance.cs          
│       ├── AssemblyInstance.cs       
│       └── ModelContainer.cs         
├── Markup/
│   ├── MarkupLine.cs                 # Single-line markup
│   └── MarkupMultiline.cs            # Multi-line markup
├── Loading/
│   ├── IModuleLoader.cs              # Loader interface
│   ├── IResourceResolver.cs          # Resource resolution abstraction
│   ├── ModuleLoader.cs               # Default implementation
│   ├── FileSystemResourceResolver.cs # File system resolver
│   ├── EmbeddedResourceResolver.cs   # Embedded resource resolver
│   └── XmlModuleParser.cs            # XML parsing logic
└── Exceptions/
    ├── MetaschemaException.cs        # Base exception
    ├── ModuleLoadException.cs        # Loading errors
    └── CircularImportException.cs    # Cycle detection
```

## Core Interfaces

### IModule

```csharp
namespace Metaschema.Core.Model;

/// <summary>
/// Represents a loaded Metaschema module with all its definitions and imports.
/// </summary>
public interface IModule
{
    /// <summary>Human-readable name for the model (schema-name).</summary>
    string Name { get; }
    
    /// <summary>Unique identifier for the module series (short-name).</summary>
    string ShortName { get; }
    
    /// <summary>Semantic version of the module (schema-version).</summary>
    string Version { get; }
    
    /// <summary>XML namespace URI for data instances (namespace).</summary>
    Uri Namespace { get; }
    
    /// <summary>Base URI for JSON Schema $schema keyword (json-base-uri).</summary>
    Uri JsonBaseUri { get; }
    
    /// <summary>Additional documentation about the module.</summary>
    MarkupMultiline? Remarks { get; }
    
    /// <summary>Source location of the module file.</summary>
    Uri Location { get; }
    
    /// <summary>Modules imported by this module.</summary>
    IReadOnlyList<IModule> ImportedModules { get; }
    
    /// <summary>Flag definitions declared in this module (not imported).</summary>
    IEnumerable<IFlagDefinition> FlagDefinitions { get; }
    
    /// <summary>Field definitions declared in this module (not imported).</summary>
    IEnumerable<IFieldDefinition> FieldDefinitions { get; }
    
    /// <summary>Assembly definitions declared in this module (not imported).</summary>
    IEnumerable<IAssemblyDefinition> AssemblyDefinitions { get; }
    
    /// <summary>
    /// Resolves a flag definition by name, checking local definitions first,
    /// then imported modules (later imports shadow earlier ones).
    /// </summary>
    IFlagDefinition? GetFlagDefinition(string name);
    
    /// <summary>
    /// Resolves a field definition by name, checking local definitions first,
    /// then imported modules (later imports shadow earlier ones).
    /// </summary>
    IFieldDefinition? GetFieldDefinition(string name);
    
    /// <summary>
    /// Resolves an assembly definition by name, checking local definitions first,
    /// then imported modules (later imports shadow earlier ones).
    /// </summary>
    IAssemblyDefinition? GetAssemblyDefinition(string name);
    
    /// <summary>
    /// Definitions exported by this module (scope=global only).
    /// </summary>
    IEnumerable<IDefinition> ExportedDefinitions { get; }
    
    /// <summary>
    /// Assembly definitions that can be document roots (have root-name).
    /// </summary>
    IEnumerable<IAssemblyDefinition> RootAssemblyDefinitions { get; }
}
```

### IDefinition (base)

```csharp
namespace Metaschema.Core.Model;

/// <summary>
/// Base interface for all Metaschema definitions (flag, field, assembly).
/// </summary>
public interface IDefinition
{
    /// <summary>Unique identifier within the module.</summary>
    string Name { get; }
    
    /// <summary>Override for the effective name in data instances.</summary>
    string? UseName { get; }
    
    /// <summary>The effective name (UseName if set, otherwise Name).</summary>
    string EffectiveName { get; }
    
    /// <summary>Human-readable label for documentation.</summary>
    string? FormalName { get; }
    
    /// <summary>Semantic description of the definition.</summary>
    MarkupLine? Description { get; }
    
    /// <summary>Visibility scope (global or local).</summary>
    Scope Scope { get; }
    
    /// <summary>Version when this definition was deprecated.</summary>
    string? DeprecatedVersion { get; }
    
    /// <summary>Additional notes and clarifications.</summary>
    MarkupMultiline? Remarks { get; }
    
    /// <summary>The module containing this definition.</summary>
    IModule ContainingModule { get; }
}
```

### IFlagDefinition

```csharp
namespace Metaschema.Core.Model;

/// <summary>
/// A flag definition represents a simple named value (leaf node).
/// Flags are like XML attributes - they have no child elements.
/// </summary>
public interface IFlagDefinition : IDefinition
{
    /// <summary>Data type name (default: "string").</summary>
    string DataTypeName { get; }
    
    /// <summary>Default value when the flag is omitted.</summary>
    string? DefaultValue { get; }
}
```

### IFieldDefinition

```csharp
namespace Metaschema.Core.Model;

/// <summary>
/// A field definition represents a value container with optional flags (edge node).
/// Fields have a value and can have flag children, but no field/assembly children.
/// </summary>
public interface IFieldDefinition : IDefinition
{
    /// <summary>Data type name (default: "string").</summary>
    string DataTypeName { get; }
    
    /// <summary>Default value when the field is omitted.</summary>
    string? DefaultValue { get; }
    
    /// <summary>Whether the field can be collapsed in JSON/YAML.</summary>
    bool IsCollapsible { get; }
    
    /// <summary>Property name for the field value in JSON.</summary>
    string? JsonValueKeyName { get; }
    
    /// <summary>Flag reference for JSON object keys in collections.</summary>
    string? JsonKeyFlagRef { get; }
    
    /// <summary>Flag instances declared on this field.</summary>
    IReadOnlyList<IFlagInstance> FlagInstances { get; }
}
```

### IAssemblyDefinition

```csharp
namespace Metaschema.Core.Model;

/// <summary>
/// An assembly definition represents a complex composite object (compositional node).
/// Assemblies have no value of their own but contain flags and a model of child elements.
/// </summary>
public interface IAssemblyDefinition : IDefinition
{
    /// <summary>Name when used as a document root element.</summary>
    string? RootName { get; }
    
    /// <summary>Whether this assembly can be a document root.</summary>
    bool IsRoot { get; }
    
    /// <summary>Flag reference for JSON object keys in collections.</summary>
    string? JsonKeyFlagRef { get; }
    
    /// <summary>Flag instances declared on this assembly.</summary>
    IReadOnlyList<IFlagInstance> FlagInstances { get; }
    
    /// <summary>Model containing child field and assembly instances.</summary>
    IModelContainer? Model { get; }
}
```

### Instance Interfaces

```csharp
namespace Metaschema.Core.Model;

/// <summary>
/// Base interface for all instances (references to definitions).
/// </summary>
public interface IInstance
{
    /// <summary>Override for formal name from the definition.</summary>
    string? FormalName { get; }
    
    /// <summary>Override for description from the definition.</summary>
    MarkupLine? Description { get; }
    
    /// <summary>Override for the effective name.</summary>
    string? UseName { get; }
    
    /// <summary>The effective name for this instance.</summary>
    string EffectiveName { get; }
    
    /// <summary>Additional notes.</summary>
    MarkupMultiline? Remarks { get; }
    
    /// <summary>Version when deprecated.</summary>
    string? DeprecatedVersion { get; }
}

/// <summary>
/// A flag instance is a reference to a flag definition within a field or assembly.
/// </summary>
public interface IFlagInstance : IInstance
{
    /// <summary>Name of the referenced flag definition.</summary>
    string Ref { get; }
    
    /// <summary>Whether this flag is required.</summary>
    bool IsRequired { get; }
    
    /// <summary>The resolved flag definition (null if unresolved).</summary>
    IFlagDefinition? ResolvedDefinition { get; }
}

/// <summary>
/// Base interface for named model instances (field and assembly instances).
/// </summary>
public interface INamedModelInstance : IInstance
{
    /// <summary>Minimum occurrences (default: 0).</summary>
    int MinOccurs { get; }
    
    /// <summary>Maximum occurrences (null = unbounded, default: 1).</summary>
    int? MaxOccurs { get; }
    
    /// <summary>Grouping configuration for collections.</summary>
    GroupAs? GroupAs { get; }
}

/// <summary>
/// A field instance is a reference to a field definition within an assembly model.
/// </summary>
public interface IFieldInstance : INamedModelInstance
{
    /// <summary>Name of the referenced field definition.</summary>
    string Ref { get; }
    
    /// <summary>XML wrapping behavior.</summary>
    XmlWrapping InXml { get; }
    
    /// <summary>The resolved field definition (null if unresolved).</summary>
    IFieldDefinition? ResolvedDefinition { get; }
}

/// <summary>
/// An assembly instance is a reference to an assembly definition within an assembly model.
/// </summary>
public interface IAssemblyInstance : INamedModelInstance
{
    /// <summary>Name of the referenced assembly definition.</summary>
    string Ref { get; }
    
    /// <summary>The resolved assembly definition (null if unresolved).</summary>
    IAssemblyDefinition? ResolvedDefinition { get; }
}
```

### IModelContainer

```csharp
namespace Metaschema.Core.Model;

/// <summary>
/// A container for model elements within an assembly definition.
/// </summary>
public interface IModelContainer
{
    /// <summary>The model elements (field instances, assembly instances, choices, etc.).</summary>
    IReadOnlyList<IModelElement> ModelElements { get; }
}

/// <summary>
/// Marker interface for elements that can appear in a model.
/// </summary>
public interface IModelElement { }

/// <summary>
/// A choice group allows mutually exclusive selection of model elements.
/// </summary>
public interface IChoiceGroup : IModelElement
{
    /// <summary>The choices available.</summary>
    IReadOnlyList<IModelElement> Choices { get; }
}
```

### Supporting Types

```csharp
namespace Metaschema.Core.Model;

/// <summary>
/// Visibility scope for definitions.
/// </summary>
public enum Scope
{
    /// <summary>Definition is available for import by other modules.</summary>
    Global,
    
    /// <summary>Definition is only usable within the defining module.</summary>
    Local
}

/// <summary>
/// XML wrapping behavior for field instances.
/// </summary>
public enum XmlWrapping
{
    /// <summary>Field is wrapped in its own element.</summary>
    Wrapped,
    
    /// <summary>Field value appears directly without wrapper.</summary>
    Unwrapped
}

/// <summary>
/// JSON grouping behavior for collections.
/// </summary>
public enum JsonGrouping
{
    /// <summary>Always an array, even with single item.</summary>
    Array,
    
    /// <summary>Single value alone; multiple as array.</summary>
    SingletonOrArray,
    
    /// <summary>Object keyed by flag value.</summary>
    ByKey
}

/// <summary>
/// XML grouping behavior for collections.
/// </summary>
public enum XmlGrouping
{
    /// <summary>Wrapper element containing children.</summary>
    Grouped,
    
    /// <summary>Children appear directly without wrapper.</summary>
    Ungrouped
}

/// <summary>
/// Grouping configuration for collection instances.
/// </summary>
public record GroupAs(
    string Name,
    JsonGrouping InJson,
    XmlGrouping InXml
);
```

## Module Loader

### IModuleLoader

```csharp
namespace Metaschema.Core.Loading;

/// <summary>
/// Loads Metaschema modules from various sources.
/// </summary>
public interface IModuleLoader
{
    /// <summary>Loads a module from a URI.</summary>
    IModule Load(Uri location);
    
    /// <summary>Loads a module from a file path.</summary>
    IModule Load(string path);
    
    /// <summary>Loads a module from a stream.</summary>
    IModule Load(Stream stream, Uri baseUri);
}
```

### IResourceResolver

```csharp
namespace Metaschema.Core.Loading;

/// <summary>
/// Resolves and opens resources for module loading.
/// </summary>
public interface IResourceResolver
{
    /// <summary>Determines if this resolver can handle the given URI.</summary>
    bool CanResolve(Uri uri);
    
    /// <summary>Opens a stream for the given URI.</summary>
    Stream Open(Uri uri);
    
    /// <summary>Resolves a relative path against a base URI.</summary>
    Uri ResolveRelative(Uri baseUri, string relativePath);
}
```

## Markup Types

```csharp
namespace Metaschema.Core.Markup;

/// <summary>
/// Single-line markup content. Will be parsed by Markdig in a later phase.
/// </summary>
public readonly record struct MarkupLine(string Value)
{
    public override string ToString() => Value;
    public static implicit operator MarkupLine(string s) => new(s);
    public static implicit operator string(MarkupLine m) => m.Value;
}

/// <summary>
/// Multi-line markup content. Will be parsed by Markdig in a later phase.
/// </summary>
public readonly record struct MarkupMultiline(string Value)
{
    public override string ToString() => Value;
    public static implicit operator MarkupMultiline(string s) => new(s);
    public static implicit operator string(MarkupMultiline m) => m.Value;
}
```

## Loading Flow

```
1. ModuleLoader.Load(path)
   │
   ├─► Resolve path to absolute URI
   │
   ├─► Check cache (return if exists)
   │
   ├─► Add URI to "loading set" (cycle detection)
   │
   ├─► Open stream via IResourceResolver
   │
   ├─► XmlModuleParser.Parse(stream, uri)
   │   │
   │   ├─► Parse header elements (schema-name, version, namespace, etc.)
   │   ├─► Parse <import> elements
   │   │   │
   │   │   └─► For each import:
   │   │       ├─► Resolve relative href
   │   │       ├─► Check "loading set" (throw CircularImportException if cycle)
   │   │       └─► Recursively load imported module
   │   │
   │   ├─► Parse <define-flag> elements
   │   ├─► Parse <define-field> elements
   │   └─► Parse <define-assembly> elements
   │
   ├─► Remove URI from "loading set"
   │
   ├─► Add to cache
   │
   └─► Return IModule
```

## Name Resolution (Shadowing)

When resolving a definition name:

1. Look in current module's local definitions
2. If not found, look in imported modules (in reverse order - last import wins)
3. Each imported module only exposes `scope="global"` definitions

```csharp
public IFlagDefinition? GetFlagDefinition(string name)
{
    // Local first (shadows imports)
    if (_localFlags.TryGetValue(name, out var local))
        return local;
    
    // Imports in reverse order (later imports shadow earlier)
    foreach (var import in ImportedModules.Reverse())
    {
        var exported = import.ExportedDefinitions
            .OfType<IFlagDefinition>()
            .FirstOrDefault(f => f.Name == name);
        if (exported != null)
            return exported;
    }
    
    return null;
}
```

## XML Namespace

The Metaschema XML namespace is:

```
http://csrc.nist.gov/ns/oscal/metaschema/1.0
```

## Test Strategy

```
test/Metaschema.Core.Tests/
├── Model/
│   ├── ModuleTests.cs
│   ├── FlagDefinitionTests.cs
│   ├── FieldDefinitionTests.cs
│   └── AssemblyDefinitionTests.cs
├── Loading/
│   ├── ModuleLoaderTests.cs
│   ├── XmlModuleParserTests.cs
│   └── CircularImportTests.cs
└── TestData/
    ├── simple-module.xml
    ├── module-with-imports/
    │   ├── main.xml
    │   └── imported.xml
    └── circular-import/
        ├── a.xml
        └── b.xml
```

## Implementation Tasks

1. **Create core interfaces** - `IModule`, `IDefinition`, `IFlagDefinition`, `IFieldDefinition`, `IAssemblyDefinition`
2. **Create instance interfaces** - `IFlagInstance`, `IFieldInstance`, `IAssemblyInstance`, `IModelContainer`
3. **Create markup types** - `MarkupLine`, `MarkupMultiline`
4. **Create supporting types** - `Scope`, `XmlWrapping`, `JsonGrouping`, `XmlGrouping`, `GroupAs`
5. **Create exceptions** - `MetaschemaException`, `ModuleLoadException`, `CircularImportException`
6. **Create loader interfaces** - `IModuleLoader`, `IResourceResolver`
7. **Implement model classes** - `Module`, `FlagDefinition`, `FieldDefinition`, `AssemblyDefinition`, instances
8. **Implement XML parser** - `XmlModuleParser`
9. **Implement module loader** - `ModuleLoader` with caching and cycle detection
10. **Implement resource resolvers** - `FileSystemResourceResolver`, `EmbeddedResourceResolver`
11. **Add tests** - Unit tests for all components
