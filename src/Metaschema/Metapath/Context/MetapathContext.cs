// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using Metaschema.Metapath.Functions;
using Metaschema.Metapath.Item;

namespace Metaschema.Metapath.Context;

/// <summary>
/// Default implementation of <see cref="IMetapathContext"/>.
/// </summary>
public sealed class MetapathContext : IMetapathContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MetapathContext"/> class.
    /// </summary>
    public MetapathContext()
        : this(Context.StaticContext.CreateDefault(), new DynamicContext())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MetapathContext"/> class with the specified contexts.
    /// </summary>
    /// <param name="staticContext">The static context.</param>
    /// <param name="dynamicContext">The dynamic context.</param>
    public MetapathContext(IStaticContext staticContext, DynamicContext dynamicContext)
    {
        StaticContext = staticContext ?? throw new ArgumentNullException(nameof(staticContext));
        DynamicContext = dynamicContext ?? throw new ArgumentNullException(nameof(dynamicContext));
    }

    /// <inheritdoc/>
    public IStaticContext StaticContext { get; }

    /// <inheritdoc/>
    IDynamicContext IMetapathContext.DynamicContext => DynamicContext;

    /// <summary>
    /// Gets the dynamic context with its concrete type for modification.
    /// </summary>
    public DynamicContext DynamicContext { get; }

    /// <inheritdoc/>
    public IMetapathContext WithContextItem(IItem contextItem)
    {
        var newDynamic = DynamicContext.Copy().WithContextItem(contextItem);
        return new MetapathContext(StaticContext, newDynamic);
    }

    /// <inheritdoc/>
    public IMetapathContext WithVariable(string name, ISequence value)
    {
        var newDynamic = DynamicContext.Copy().WithVariable(name, value);
        return new MetapathContext(StaticContext, newDynamic);
    }

    /// <summary>
    /// Creates a new context with the specified node as the context item.
    /// </summary>
    /// <param name="node">The context node.</param>
    /// <returns>A new context.</returns>
    public MetapathContext ForNode(INodeItem node) => new MetapathContext(StaticContext, DynamicContext.Copy().WithContextItem(node));

    /// <summary>
    /// Creates a default evaluation context.
    /// </summary>
    /// <returns>A new default context.</returns>
    public static MetapathContext Create() => new();

    /// <summary>
    /// Creates an evaluation context with the specified function library.
    /// </summary>
    /// <param name="functionLibrary">The function library.</param>
    /// <returns>A new context with the specified function library.</returns>
    public static MetapathContext Create(IFunctionLibrary functionLibrary)
    {
        var staticCtx = new StaticContext(functionLibrary);
        staticCtx.RegisterNamespace("fn", Context.StaticContext.MetapathFunctionNamespace);
        staticCtx.RegisterNamespace("mp", Context.StaticContext.MetapathFunctionNamespace);
        staticCtx.RegisterNamespace("xs", "http://www.w3.org/2001/XMLSchema");
        return new MetapathContext(staticCtx, new DynamicContext());
    }
}
