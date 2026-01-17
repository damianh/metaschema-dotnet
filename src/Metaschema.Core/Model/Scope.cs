// Licensed under the MIT License.

namespace Metaschema.Core.Model;

/// <summary>
/// Visibility scope for definitions.
/// </summary>
public enum Scope
{
    /// <summary>Definition is available for import by other modules.</summary>
    Global,

    /// <summary>Definition is only usable within the defining module.</summary>
    Local
}
