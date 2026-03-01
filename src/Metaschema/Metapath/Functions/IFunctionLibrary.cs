// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using Metaschema.Metapath.Item;

namespace Metaschema.Metapath.Functions;

/// <summary>
/// Represents a library of Metapath functions.
/// </summary>
public interface IFunctionLibrary
{
    /// <summary>
    /// Gets a function by name and arity.
    /// </summary>
    /// <param name="name">The function name.</param>
    /// <param name="arity">The number of arguments.</param>
    /// <returns>The function, or <c>null</c> if not found.</returns>
    IMetapathFunction? GetFunction(string name, int arity);

    /// <summary>
    /// Gets a function by qualified name and arity.
    /// </summary>
    /// <param name="namespaceUri">The namespace URI.</param>
    /// <param name="localName">The local name.</param>
    /// <param name="arity">The number of arguments.</param>
    /// <returns>The function, or <c>null</c> if not found.</returns>
    IMetapathFunction? GetFunction(string? namespaceUri, string localName, int arity);

    /// <summary>
    /// Registers a function in the library.
    /// </summary>
    /// <param name="metapathFunction">The function to register.</param>
    void RegisterFunction(IMetapathFunction metapathFunction);

    /// <summary>
    /// Gets all registered functions.
    /// </summary>
    /// <returns>All registered functions.</returns>
    IEnumerable<IMetapathFunction> GetAllFunctions();
}

/// <summary>
/// Represents a Metapath function that can be invoked during expression evaluation.
/// </summary>
public interface IMetapathFunction
{
    /// <summary>
    /// Gets the function name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the namespace URI for this function.
    /// </summary>
    string? NamespaceUri { get; }

    /// <summary>
    /// Gets the number of parameters this function accepts.
    /// A value of -1 indicates variable arity.
    /// </summary>
    int Arity { get; }

    /// <summary>
    /// Gets the minimum number of arguments required.
    /// </summary>
    int MinArity { get; }

    /// <summary>
    /// Gets the maximum number of arguments accepted.
    /// A value of -1 indicates unlimited.
    /// </summary>
    int MaxArity { get; }

    /// <summary>
    /// Invokes the function with the specified arguments.
    /// </summary>
    /// <param name="context">The evaluation context.</param>
    /// <param name="arguments">The function arguments.</param>
    /// <returns>The function result.</returns>
    ISequence Invoke(IMetapathContext context, IReadOnlyList<ISequence> arguments);
}
