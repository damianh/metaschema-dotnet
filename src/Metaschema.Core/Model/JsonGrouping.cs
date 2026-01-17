// Licensed under the MIT License.

namespace Metaschema.Core.Model;

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
