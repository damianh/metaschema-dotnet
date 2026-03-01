// Licensed under the MIT License.

using System.Collections.Concurrent;
using Metaschema.Model;

namespace Metaschema.Loading;

/// <summary>
/// Loads Metaschema modules from XML files with caching and cycle detection.
/// </summary>
public sealed class ModuleLoader : IModuleLoader
{
    private readonly ConcurrentDictionary<Uri, MetaschemaModule> _cache = new();
    private readonly IResourceResolver _resolver;

    // Thread-local loading set for cycle detection
    private static readonly AsyncLocal<HashSet<Uri>?> LoadingSet = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleLoader"/> class
    /// with the default file system resource resolver.
    /// </summary>
    public ModuleLoader()
        : this(new FileSystemResourceResolver())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleLoader"/> class.
    /// </summary>
    /// <param name="resolver">The resource resolver to use.</param>
    public ModuleLoader(IResourceResolver resolver)
    {
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    }

    /// <inheritdoc />
    public MetaschemaModule Load(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var uri = new Uri(fullPath);
        return Load(uri);
    }

    /// <inheritdoc />
    public MetaschemaModule Load(Uri location)
    {
        // Normalize the URI
        location = NormalizeUri(location);

        // Check cache first
        if (_cache.TryGetValue(location, out var cached))
        {
            return cached;
        }

        // Get or create the loading set for cycle detection
        var loadingSet = LoadingSet.Value ??= [];

        // Check for circular import
        if (loadingSet.Contains(location))
        {
            var chain = loadingSet.Append(location).ToList();
            throw new CircularImportException(chain);
        }

        // Add to loading set
        loadingSet.Add(location);

        try
        {
            // Open and parse the module
            using var stream = _resolver.Open(location);
            return LoadFromStream(stream, location);
        }
        finally
        {
            // Remove from loading set
            loadingSet.Remove(location);
        }
    }

    /// <inheritdoc />
    public MetaschemaModule Load(Stream stream, Uri baseUri)
    {
        return LoadFromStream(stream, baseUri);
    }

    private MetaschemaModule LoadFromStream(Stream stream, Uri location)
    {
        // Create the parser with a callback to load imports
        var parser = new XmlModuleParser(LoadImport);

        var module = parser.Parse(stream, location);

        // Cache the module
        _cache.TryAdd(location, module);

        return module;

        MetaschemaModule LoadImport(Uri importUri)
        {
            return Load(importUri);
        }
    }

    private static Uri NormalizeUri(Uri uri)
    {
        if (uri.IsFile)
        {
            // Normalize file paths to absolute paths
            var path = Path.GetFullPath(uri.LocalPath);
            return new Uri(path);
        }

        return uri;
    }

    /// <summary>
    /// Clears the module cache.
    /// </summary>
    public void ClearCache() => _cache.Clear();
}
