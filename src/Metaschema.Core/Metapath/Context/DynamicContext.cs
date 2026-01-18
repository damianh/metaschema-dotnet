// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Metaschema.Core.Metapath.Item;

namespace Metaschema.Core.Metapath.Context;

/// <summary>
/// Default implementation of <see cref="IDynamicContext"/>.
/// </summary>
public sealed class DynamicContext : IDynamicContext
{
    private readonly Dictionary<string, ISequence> _variables = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicContext"/> class.
    /// </summary>
    public DynamicContext()
    {
        CurrentDateTime = DateTimeOffset.Now;
        ImplicitTimezone = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
    }

    /// <inheritdoc/>
    public IItem? ContextItem { get; private set; }

    /// <inheritdoc/>
    public int ContextPosition { get; private set; } = 1;

    /// <inheritdoc/>
    public int ContextSize { get; private set; } = 1;

    /// <inheritdoc/>
    public DateTimeOffset CurrentDateTime { get; }

    /// <inheritdoc/>
    public TimeSpan ImplicitTimezone { get; }

    /// <inheritdoc/>
    public ISequence GetVariable(string name)
    {
        if (TryGetVariable(name, out var value) && value is not null)
        {
            return value;
        }
        throw new MetapathException($"Variable '${name}' is not defined.");
    }

    /// <inheritdoc/>
    public bool TryGetVariable(string name, out ISequence? value)
    {
        ArgumentNullException.ThrowIfNull(name);
        return _variables.TryGetValue(name, out value);
    }

    /// <summary>
    /// Sets the context item.
    /// </summary>
    /// <param name="item">The context item.</param>
    /// <returns>This context for fluent chaining.</returns>
    public DynamicContext WithContextItem(IItem? item)
    {
        ContextItem = item;
        return this;
    }

    /// <summary>
    /// Sets the context position and size for sequence iteration.
    /// </summary>
    /// <param name="position">The position (1-based).</param>
    /// <param name="size">The size of the sequence.</param>
    /// <returns>This context for fluent chaining.</returns>
    public DynamicContext WithPosition(int position, int size)
    {
        ContextPosition = position;
        ContextSize = size;
        return this;
    }

    /// <summary>
    /// Binds a variable in this context.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <param name="value">The variable value.</param>
    /// <returns>This context for fluent chaining.</returns>
    public DynamicContext WithVariable(string name, ISequence value)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(value);
        _variables[name] = value;
        return this;
    }

    /// <summary>
    /// Creates a copy of this dynamic context.
    /// </summary>
    /// <returns>A copy of this context.</returns>
    public DynamicContext Copy()
    {
        var copy = new DynamicContext
        {
            ContextItem = ContextItem,
            ContextPosition = ContextPosition,
            ContextSize = ContextSize
        };
        foreach (var (name, value) in _variables)
        {
            copy._variables[name] = value;
        }
        return copy;
    }
}
