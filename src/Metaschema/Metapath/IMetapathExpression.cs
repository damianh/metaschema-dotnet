// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Metaschema.Metapath.Item;

namespace Metaschema.Metapath;

/// <summary>
/// Represents a compiled Metapath expression that can be evaluated against a context.
/// </summary>
public interface IMetapathExpression
{
    /// <summary>
    /// Gets the original expression string.
    /// </summary>
    string Expression { get; }

    /// <summary>
    /// Evaluates the expression against the specified context, returning a sequence of results.
    /// </summary>
    /// <param name="context">The evaluation context.</param>
    /// <returns>The result sequence.</returns>
    ISequence Evaluate(IMetapathContext context);

    /// <summary>
    /// Evaluates the expression against the specified context node, returning a sequence of results.
    /// </summary>
    /// <param name="contextItem">The context item to evaluate against.</param>
    /// <returns>The result sequence.</returns>
    ISequence Evaluate(INodeItem contextItem);

    /// <summary>
    /// Evaluates the expression and returns a single item result, or <c>null</c> if the result is empty.
    /// </summary>
    /// <param name="context">The evaluation context.</param>
    /// <returns>The single result item, or <c>null</c>.</returns>
    /// <exception cref="MetapathException">Thrown if the result contains more than one item.</exception>
    IItem? EvaluateSingle(IMetapathContext context);

    /// <summary>
    /// Evaluates the expression and returns the effective boolean value of the result.
    /// </summary>
    /// <param name="context">The evaluation context.</param>
    /// <returns>The effective boolean value.</returns>
    bool EvaluateBoolean(IMetapathContext context);

    /// <summary>
    /// Evaluates the expression and returns the string value of the result.
    /// </summary>
    /// <param name="context">The evaluation context.</param>
    /// <returns>The string value, or <c>null</c> if the result is empty.</returns>
    string? EvaluateString(IMetapathContext context);
}
