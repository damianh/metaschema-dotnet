// Licensed under the MIT License.

using Metaschema.Core.Model;

namespace Metaschema.Core.Loading;

/// <summary>
/// Loads Metaschema modules from various sources with caching and cycle detection.
/// </summary>
public interface IModuleLoader
{
    /// <summary>
    /// Loads a module from a URI.
    /// </summary>
    /// <param name="location">The URI of the module to load.</param>
    /// <returns>The loaded module.</returns>
    /// <exception cref="ModuleLoadException">Thrown when the module cannot be loaded.</exception>
    /// <exception cref="CircularImportException">Thrown when a circular import is detected.</exception>
    MetaschemaModule Load(Uri location);

    /// <summary>
    /// Loads a module from a file path.
    /// </summary>
    /// <param name="path">The file path of the module to load.</param>
    /// <returns>The loaded module.</returns>
    /// <exception cref="ModuleLoadException">Thrown when the module cannot be loaded.</exception>
    /// <exception cref="CircularImportException">Thrown when a circular import is detected.</exception>
    MetaschemaModule Load(string path);

    /// <summary>
    /// Loads a module from a stream.
    /// </summary>
    /// <param name="stream">The stream containing the module XML.</param>
    /// <param name="baseUri">The base URI for resolving relative imports.</param>
    /// <returns>The loaded module.</returns>
    /// <exception cref="ModuleLoadException">Thrown when the module cannot be loaded.</exception>
    /// <exception cref="CircularImportException">Thrown when a circular import is detected.</exception>
    MetaschemaModule Load(Stream stream, Uri baseUri);
}
