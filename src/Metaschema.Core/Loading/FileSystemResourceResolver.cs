// Licensed under the MIT License.

namespace Metaschema.Core.Loading;

/// <summary>
/// Resolves file system URIs for module loading.
/// </summary>
public sealed class FileSystemResourceResolver : IResourceResolver
{
    /// <inheritdoc />
    public bool CanResolve(Uri uri) =>
        uri.IsFile || uri.IsAbsoluteUri == false;

    /// <inheritdoc />
    public Stream Open(Uri uri)
    {
        var path = uri.IsAbsoluteUri ? uri.LocalPath : uri.OriginalString;

        try
        {
            return File.OpenRead(path);
        }
        catch (FileNotFoundException ex)
        {
            throw new ModuleLoadException($"File not found: {path}", uri, ex);
        }
        catch (DirectoryNotFoundException ex)
        {
            throw new ModuleLoadException($"Directory not found: {path}", uri, ex);
        }
        catch (IOException ex)
        {
            throw new ModuleLoadException($"Failed to open file: {path}", uri, ex);
        }
    }

    /// <inheritdoc />
    public Uri ResolveRelative(Uri baseUri, string relativePath)
    {
        if (Uri.TryCreate(relativePath, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri;
        }

        // For file URIs, resolve relative to the directory containing the base file
        if (baseUri.IsFile)
        {
            var baseDir = Path.GetDirectoryName(baseUri.LocalPath)
                ?? throw new ModuleLoadException(
                    $"Cannot determine directory for base URI: {baseUri}",
                    baseUri);
            var fullPath = Path.GetFullPath(Path.Combine(baseDir, relativePath));
            return new Uri(fullPath);
        }

        // Fall back to Uri combination
        return new Uri(baseUri, relativePath);
    }
}
