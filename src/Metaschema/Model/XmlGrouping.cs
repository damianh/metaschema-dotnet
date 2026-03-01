// Licensed under the MIT License.

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
