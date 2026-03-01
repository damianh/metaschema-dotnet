// Licensed under the MIT License.

namespace Metaschema.Constraints;

/// <summary>
/// Indicates the source of a constraint definition.
/// </summary>
public enum ConstraintSource
{
    /// <summary>
    /// Constraint is defined inline within the Metaschema model.
    /// </summary>
    Model,

    /// <summary>
    /// Constraint is defined in an external constraint set file.
    /// </summary>
    External
}
