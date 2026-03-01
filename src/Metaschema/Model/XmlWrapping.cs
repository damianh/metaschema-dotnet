// Licensed under the MIT License.

namespace Metaschema.Model;

/// <summary>
/// XML wrapping behavior for field instances.
/// </summary>
public enum XmlWrapping
{
    /// <summary>Field is wrapped in its own element.</summary>
    Wrapped,

    /// <summary>Field value appears directly without wrapper.</summary>
    Unwrapped
}
