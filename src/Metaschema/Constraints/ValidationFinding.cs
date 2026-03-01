// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using Metaschema.Metapath.Item;

namespace Metaschema.Constraints;

/// <summary>
/// Represents a single validation finding from constraint evaluation.
/// </summary>
/// <param name="Severity">The severity level of the finding.</param>
/// <param name="Location">The Metapath location of the node that caused the finding.</param>
/// <param name="Constraint">The constraint that was violated.</param>
/// <param name="Message">The formatted message describing the finding.</param>
/// <param name="Node">The node that was validated.</param>
public sealed record ValidationFinding(
    ConstraintLevel Severity,
    string Location,
    IConstraint Constraint,
    string Message,
    INodeItem? Node = null)
{
    /// <summary>
    /// Creates a validation finding with a custom message.
    /// </summary>
    public static ValidationFinding Create(
        IConstraint constraint,
        INodeItem node,
        string? customMessage = null)
    {
        var message = customMessage ?? constraint.Message ?? GetDefaultMessage(constraint);
        return new ValidationFinding(
            constraint.Level,
            node.GetPath(),
            constraint,
            message,
            node);
    }

    /// <summary>
    /// Creates a validation finding with formatted message arguments.
    /// </summary>
    public static ValidationFinding Create(
        IConstraint constraint,
        INodeItem node,
        string messageFormat,
        params object[] args)
    {
        var message = string.Format(System.Globalization.CultureInfo.InvariantCulture, messageFormat, args);
        return new ValidationFinding(
            constraint.Level,
            node.GetPath(),
            constraint,
            message,
            node);
    }

    private static string GetDefaultMessage(IConstraint constraint) => constraint switch
    {
        IAllowedValuesConstraint => "Value is not in the allowed values list",
        IMatchesConstraint m => $"Value does not match pattern '{m.Pattern}'",
        IExpectConstraint e => $"Assertion failed: {e.Test}",
        IIndexConstraint => "Index constraint violation",
        IIndexHasKeyConstraint ihk => $"Key not found in index '{ihk.IndexName}'",
        IUniqueConstraint => "Uniqueness constraint violated",
        ICardinalityConstraint => "Cardinality constraint violated",
        _ => "Constraint violated"
    };

    /// <summary>
    /// Returns a string representation of this finding.
    /// </summary>
    public override string ToString() =>
        $"[{Severity}] {Location}: {Message}";
}
