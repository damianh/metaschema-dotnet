// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Metaschema.Metapath.Item;

/// <summary>
/// Represents a sequence of items, which is the fundamental data structure in Metapath.
/// A sequence is an ordered collection of zero or more items.
/// </summary>
public interface ISequence : IEnumerable<IItem>
{
    /// <summary>
    /// Gets the number of items in the sequence.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets a value indicating whether the sequence is empty.
    /// </summary>
    bool IsEmpty { get; }

    /// <summary>
    /// Gets the first item in the sequence, or <c>null</c> if the sequence is empty.
    /// </summary>
    IItem? FirstOrDefault { get; }

    /// <summary>
    /// Gets the effective boolean value of the sequence.
    /// </summary>
    /// <returns>The effective boolean value.</returns>
    /// <remarks>
    /// The effective boolean value is determined as follows:
    /// - An empty sequence is false
    /// - A sequence starting with a node is true
    /// - A single atomic value is converted to boolean
    /// - Other sequences throw an exception
    /// </remarks>
    bool GetEffectiveBooleanValue();

    /// <summary>
    /// Creates a new sequence with the specified item appended.
    /// </summary>
    /// <param name="item">The item to append.</param>
    /// <returns>A new sequence with the item appended.</returns>
    ISequence Append(IItem item);

    /// <summary>
    /// Creates a new sequence by concatenating this sequence with another.
    /// </summary>
    /// <param name="other">The other sequence.</param>
    /// <returns>A new concatenated sequence.</returns>
    ISequence Concat(ISequence other);
}
