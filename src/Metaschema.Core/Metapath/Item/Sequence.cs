// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace Metaschema.Core.Metapath.Item;

/// <summary>
/// Default implementation of <see cref="ISequence"/>.
/// </summary>
public sealed class Sequence : ISequence
{
    /// <summary>
    /// Gets an empty sequence.
    /// </summary>
    public static readonly ISequence Empty = new Sequence([]);

    private readonly List<IItem> _items;

    /// <summary>
    /// Initializes a new instance of the <see cref="Sequence"/> class.
    /// </summary>
    public Sequence()
    {
        _items = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Sequence"/> class with the specified items.
    /// </summary>
    /// <param name="items">The items in the sequence.</param>
    public Sequence(IEnumerable<IItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        _items = items.ToList();
    }

    /// <summary>
    /// Creates a sequence from a single item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>A sequence containing the single item.</returns>
    public static ISequence Of(IItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return new Sequence([item]);
    }

    /// <summary>
    /// Creates a sequence from multiple items.
    /// </summary>
    /// <param name="items">The items.</param>
    /// <returns>A sequence containing the items.</returns>
    public static ISequence Of(params IItem[] items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return items.Length == 0 ? Empty : new Sequence(items);
    }

    /// <inheritdoc/>
    public int Count => _items.Count;

    /// <inheritdoc/>
    public bool IsEmpty => _items.Count == 0;

    /// <inheritdoc/>
    public IItem? FirstOrDefault => _items.Count > 0 ? _items[0] : null;

    /// <inheritdoc/>
    public bool GetEffectiveBooleanValue()
    {
        if (_items.Count == 0)
        {
            return false;
        }

        var first = _items[0];

        // A sequence starting with a node is true
        if (first is INodeItem)
        {
            return true;
        }

        // A single atomic value is converted to boolean
        if (_items.Count == 1 && first is IAtomicItem)
        {
            return first.GetEffectiveBooleanValue();
        }

        // Other sequences throw an exception
        throw new MetapathException(
            "The effective boolean value is not defined for a sequence of more than one atomic value.");
    }

    /// <inheritdoc/>
    public ISequence Append(IItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var newItems = new List<IItem>(_items) { item };
        return new Sequence(newItems);
    }

    /// <inheritdoc/>
    public ISequence Concat(ISequence other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (IsEmpty) return other;
        if (other.IsEmpty) return this;
        return new Sequence(_items.Concat(other));
    }

    /// <inheritdoc/>
    public IEnumerator<IItem> GetEnumerator() => _items.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
