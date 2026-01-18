// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Metaschema.Core.Metapath.Functions;

namespace Metaschema.Core.Metapath.Context;

/// <summary>
/// Default implementation of <see cref="IStaticContext"/>.
/// </summary>
public sealed class StaticContext : IStaticContext
{
    private readonly Dictionary<string, string> _namespaces = new(StringComparer.Ordinal);

    /// <summary>
    /// The default Metapath function namespace.
    /// </summary>
    public const string MetapathFunctionNamespace = "http://csrc.nist.gov/ns/metaschema/metapath-functions";

    /// <summary>
    /// Initializes a new instance of the <see cref="StaticContext"/> class with built-in functions.
    /// </summary>
    public StaticContext()
    {
        FunctionLibrary = Functions.FunctionLibrary.Default;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StaticContext"/> class with the specified function library.
    /// </summary>
    /// <param name="functionLibrary">The function library.</param>
    public StaticContext(IFunctionLibrary functionLibrary)
    {
        FunctionLibrary = functionLibrary ?? throw new ArgumentNullException(nameof(functionLibrary));
    }

    /// <inheritdoc/>
    public string? DefaultElementNamespace { get; set; }

    /// <inheritdoc/>
    public string? DefaultFunctionNamespace { get; set; } = MetapathFunctionNamespace;

    /// <inheritdoc/>
    public IFunctionLibrary FunctionLibrary { get; }

    /// <inheritdoc/>
    public Uri? BaseUri { get; set; }

    /// <inheritdoc/>
    public string? GetNamespaceUri(string prefix)
    {
        ArgumentNullException.ThrowIfNull(prefix);
        return _namespaces.TryGetValue(prefix, out var uri) ? uri : null;
    }

    /// <inheritdoc/>
    public void RegisterNamespace(string prefix, string namespaceUri)
    {
        ArgumentNullException.ThrowIfNull(prefix);
        ArgumentNullException.ThrowIfNull(namespaceUri);
        _namespaces[prefix] = namespaceUri;
    }

    /// <summary>
    /// Creates a default static context with standard namespaces registered.
    /// </summary>
    /// <returns>A new static context with standard configuration.</returns>
    public static StaticContext CreateDefault()
    {
        var context = new StaticContext();
        context.RegisterNamespace("fn", MetapathFunctionNamespace);
        context.RegisterNamespace("mp", MetapathFunctionNamespace);
        context.RegisterNamespace("xs", "http://www.w3.org/2001/XMLSchema");
        return context;
    }
}
