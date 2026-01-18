// Licensed under the MIT License.

namespace Metaschema.Core.Constraints;

/// <summary>
/// Defines the severity level for constraint violations.
/// </summary>
public enum ConstraintLevel
{
    /// <summary>
    /// Critical violation that prevents processing.
    /// </summary>
    Critical = 0,

    /// <summary>
    /// Error that should be corrected but doesn't prevent processing.
    /// </summary>
    Error = 1,

    /// <summary>
    /// Warning that indicates a potential issue.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Informational message for best practices or recommendations.
    /// </summary>
    Informational = 3
}
