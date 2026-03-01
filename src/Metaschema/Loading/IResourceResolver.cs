// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

namespace Metaschema.Loading;

/// <summary>
/// Resolves and opens resources for module loading.
/// </summary>
public interface IResourceResolver
{
    /// <summary>
    /// Determines if this resolver can handle the given URI.
    /// </summary>
    /// <param name="uri">The URI to check.</param>
    /// <returns>True if this resolver can handle the URI; otherwise, false.</returns>
    bool CanResolve(Uri uri);

    /// <summary>
    /// Opens a stream for the given URI.
    /// </summary>
    /// <param name="uri">The URI to open.</param>
    /// <returns>A stream for reading the resource.</returns>
    /// <exception cref="ModuleLoadException">Thrown when the resource cannot be opened.</exception>
    Stream Open(Uri uri);

    /// <summary>
    /// Resolves a relative path against a base URI.
    /// </summary>
    /// <param name="baseUri">The base URI to resolve against.</param>
    /// <param name="relativePath">The relative path to resolve.</param>
    /// <returns>The resolved absolute URI.</returns>
    Uri ResolveRelative(Uri baseUri, string relativePath);
}
