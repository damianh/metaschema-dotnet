// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

namespace Metaschema.Loading;

/// <summary>
/// Exception thrown when a circular import is detected during module loading.
/// </summary>
public class CircularImportException : ModuleLoadException
{
    /// <summary>
    /// Gets the import chain that forms the cycle.
    /// </summary>
    public IReadOnlyList<Uri> ImportChain { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CircularImportException"/> class.
    /// </summary>
    /// <param name="importChain">The import chain that forms the cycle.</param>
    public CircularImportException(IReadOnlyList<Uri> importChain)
        : base(BuildMessage(importChain), importChain[^1]) => ImportChain = importChain;

    private static string BuildMessage(IReadOnlyList<Uri> importChain)
    {
        var chain = string.Join(" -> ", importChain.Select(u => u.ToString()));
        return $"Circular import detected: {chain}";
    }
}
