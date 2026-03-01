// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

namespace Metaschema.Metapath.Item;

/// <summary>
/// Represents a single item in a Metapath sequence.
/// This is the base type for all Metapath values including atomic values and nodes.
/// </summary>
public interface IItem
{
    /// <summary>
    /// Gets the typed value of this item.
    /// </summary>
    /// <returns>The typed value.</returns>
    object? GetTypedValue();

    /// <summary>
    /// Gets the string value of this item.
    /// </summary>
    /// <returns>The string value.</returns>
    string GetStringValue();

    /// <summary>
    /// Gets the effective boolean value of this item.
    /// </summary>
    /// <returns>The effective boolean value.</returns>
    bool GetEffectiveBooleanValue();
}
