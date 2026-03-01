// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

namespace Metaschema.Model;

/// <summary>
/// XML grouping behavior for collections.
/// </summary>
public enum XmlGrouping
{
    /// <summary>Wrapper element containing children.</summary>
    Grouped,

    /// <summary>Children appear directly without wrapper.</summary>
    Ungrouped
}
