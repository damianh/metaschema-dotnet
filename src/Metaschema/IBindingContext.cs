// Licensed under the MIT License.

using Metaschema.Model;
using Metaschema.Nodes;

namespace Metaschema;

/// <summary>
/// Provides the context for binding Metaschema modules to serialization operations.
/// </summary>
public interface IBindingContext
{
    /// <summary>
    /// Gets the registered modules.
    /// </summary>
    IEnumerable<MetaschemaModule> Modules { get; }

    /// <summary>
    /// Registers a module with this binding context.
    /// </summary>
    /// <param name="metaschemaModule">The module to register.</param>
    void RegisterModule(MetaschemaModule metaschemaModule);

    /// <summary>
    /// Gets a serializer for the specified format.
    /// </summary>
    /// <param name="format">The serialization format.</param>
    /// <returns>A serializer for the format.</returns>
    ISerializer GetSerializer(Format format);

    /// <summary>
    /// Gets a deserializer for the specified format.
    /// </summary>
    /// <param name="format">The serialization format.</param>
    /// <returns>A deserializer for the format.</returns>
    IDeserializer GetDeserializer(Format format);

    /// <summary>
    /// Creates a new bound loader for loading content with format detection.
    /// </summary>
    /// <returns>A new bound loader.</returns>
    IBoundLoader NewBoundLoader();

    /// <summary>
    /// Resolves an assembly definition by root name across all registered modules.
    /// </summary>
    /// <param name="rootName">The root element name.</param>
    /// <returns>The assembly definition, or null if not found.</returns>
    AssemblyDefinition? ResolveRootAssembly(string rootName);

    /// <summary>
    /// Resolves an assembly definition by root name and namespace across all registered modules.
    /// </summary>
    /// <param name="rootName">The root element name.</param>
    /// <param name="namespaceUri">The XML namespace URI.</param>
    /// <returns>The assembly definition, or null if not found.</returns>
    AssemblyDefinition? ResolveRootAssembly(string rootName, Uri namespaceUri);
}

/// <summary>
/// Loads Metaschema-based content with format detection.
/// </summary>
public interface IBoundLoader
{
    /// <summary>
    /// Gets the formats enabled for this loader.
    /// </summary>
    IEnumerable<Format> EnabledFormats { get; }

    /// <summary>
    /// Detects the format of the content in a stream.
    /// </summary>
    /// <param name="input">The input stream (must be seekable).</param>
    /// <returns>The detected format.</returns>
    Format DetectFormat(Stream input);

    /// <summary>
    /// Loads content from a stream with automatic format detection.
    /// </summary>
    /// <param name="input">The input stream.</param>
    /// <returns>The loaded document node.</returns>
    DocumentNode Load(Stream input);

    /// <summary>
    /// Loads content from a file with automatic format detection.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>The loaded document node.</returns>
    DocumentNode Load(string path);

    /// <summary>
    /// Loads content from a stream with a specified format.
    /// </summary>
    /// <param name="input">The input stream.</param>
    /// <param name="format">The content format.</param>
    /// <returns>The loaded document node.</returns>
    DocumentNode Load(Stream input, Format format);
}
