// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Metaschema.Core.Metapath.Item;

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
