// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

namespace Metaschema.Model;

/// <summary>
/// JSON grouping behavior for collections.
/// </summary>
public enum JsonGrouping
{
    /// <summary>Always an array, even with single item.</summary>
    Array,

    /// <summary>Single value alone; multiple as array.</summary>
    SingletonOrArray,

    /// <summary>Object keyed by flag value.</summary>
    ByKey
}
