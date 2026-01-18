// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Metaschema.Core.Metapath.Item;

namespace Metaschema.Core.Metapath;

/// <summary>
/// Provides the context for evaluating Metapath expressions.
/// This includes the static context (namespaces, functions) and dynamic context (variables, context item).
/// </summary>
public interface IMetapathContext
{
    /// <summary>
    /// Gets the static context which contains compile-time information.
    /// </summary>
    IStaticContext StaticContext { get; }

    /// <summary>
    /// Gets the dynamic context which contains runtime information.
    /// </summary>
    IDynamicContext DynamicContext { get; }

    /// <summary>
    /// Creates a new context with the specified context item.
    /// </summary>
    /// <param name="contextItem">The context item.</param>
    /// <returns>A new context with the specified context item.</returns>
    IMetapathContext WithContextItem(IItem contextItem);

    /// <summary>
    /// Creates a new context with the specified variable binding.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <param name="value">The variable value.</param>
    /// <returns>A new context with the variable bound.</returns>
    IMetapathContext WithVariable(string name, ISequence value);
}

/// <summary>
/// Provides compile-time context for Metapath expressions.
/// </summary>
public interface IStaticContext
{
    /// <summary>
    /// Gets the default namespace URI for elements.
    /// </summary>
    string? DefaultElementNamespace { get; }

    /// <summary>
    /// Gets the default namespace URI for functions.
    /// </summary>
    string? DefaultFunctionNamespace { get; }

    /// <summary>
    /// Gets the namespace URI for a prefix.
    /// </summary>
    /// <param name="prefix">The namespace prefix.</param>
    /// <returns>The namespace URI, or <c>null</c> if not found.</returns>
    string? GetNamespaceUri(string prefix);

    /// <summary>
    /// Registers a namespace prefix.
    /// </summary>
    /// <param name="prefix">The prefix.</param>
    /// <param name="namespaceUri">The namespace URI.</param>
    void RegisterNamespace(string prefix, string namespaceUri);

    /// <summary>
    /// Gets the function library for resolving function calls.
    /// </summary>
    Functions.IFunctionLibrary FunctionLibrary { get; }
}

/// <summary>
/// Provides runtime context for Metapath expressions.
/// </summary>
public interface IDynamicContext
{
    /// <summary>
    /// Gets the current context item (`.`).
    /// </summary>
    IItem? ContextItem { get; }

    /// <summary>
    /// Gets the current context position (in a sequence iteration).
    /// </summary>
    int ContextPosition { get; }

    /// <summary>
    /// Gets the current context size (in a sequence iteration).
    /// </summary>
    int ContextSize { get; }

    /// <summary>
    /// Gets the value of a variable.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <returns>The variable value.</returns>
    /// <exception cref="MetapathException">Thrown if the variable is not defined.</exception>
    ISequence GetVariable(string name);

    /// <summary>
    /// Tries to get the value of a variable.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <param name="value">The variable value, if found.</param>
    /// <returns><c>true</c> if the variable was found; otherwise, <c>false</c>.</returns>
    bool TryGetVariable(string name, out ISequence? value);

    /// <summary>
    /// Gets the current date and time.
    /// </summary>
    DateTimeOffset CurrentDateTime { get; }

    /// <summary>
    /// Gets the implicit timezone offset.
    /// </summary>
    TimeSpan ImplicitTimezone { get; }
}
