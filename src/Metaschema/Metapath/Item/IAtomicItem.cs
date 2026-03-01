// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

namespace Metaschema.Metapath.Item;

/// <summary>
/// Represents an atomic value in a Metapath expression.
/// Atomic values are indivisible units such as strings, numbers, and booleans.
/// </summary>
public interface IAtomicItem : IItem
{
    /// <summary>
    /// Gets the underlying .NET value.
    /// </summary>
    object Value { get; }

    /// <summary>
    /// Gets the Metaschema data type name for this atomic item.
    /// </summary>
    string TypeName { get; }
}
