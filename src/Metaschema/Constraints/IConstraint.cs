// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

namespace Metaschema.Constraints;

/// <summary>
/// Base interface for all Metaschema constraints.
/// </summary>
public interface IConstraint
{
    /// <summary>
    /// Gets the unique identifier for this constraint.
    /// </summary>
    string? Id { get; }

    /// <summary>
    /// Gets the severity level for violations of this constraint.
    /// </summary>
    ConstraintLevel Level { get; }

    /// <summary>
    /// Gets the Metapath expression that targets nodes to validate.
    /// If null, the constraint applies to the containing definition.
    /// </summary>
    string? Target { get; }

    /// <summary>
    /// Gets the custom message to display when this constraint is violated.
    /// </summary>
    string? Message { get; }

    /// <summary>
    /// Gets additional remarks about this constraint.
    /// </summary>
    string? Remarks { get; }

    /// <summary>
    /// Gets the source of this constraint (model or external).
    /// </summary>
    ConstraintSource Source { get; }
}

/// <summary>
/// Constraint that restricts values to an enumerated set.
/// </summary>
public interface IAllowedValuesConstraint : IConstraint
{
    /// <summary>
    /// Gets the allowed values with their descriptions.
    /// </summary>
    IReadOnlyList<AllowedValue> AllowedValues { get; }

    /// <summary>
    /// Gets whether other values not in the list are permitted.
    /// </summary>
    bool AllowOther { get; }

    /// <summary>
    /// Gets whether the allowed values extend (true) or replace (false) inherited values.
    /// </summary>
    bool Extensible { get; }
}

/// <summary>
/// Represents an allowed enumeration value.
/// </summary>
/// <param name="Value">The allowed value.</param>
/// <param name="Description">Description of what this value means.</param>
/// <param name="DeprecatedVersion">Version when this value was deprecated, if any.</param>
public readonly record struct AllowedValue(
    string Value,
    string? Description = null,
    string? DeprecatedVersion = null);

/// <summary>
/// Constraint that validates values against a regular expression pattern.
/// </summary>
public interface IMatchesConstraint : IConstraint
{
    /// <summary>
    /// Gets the regular expression pattern to match.
    /// </summary>
    string? Pattern { get; }

    /// <summary>
    /// Gets the data type to validate against.
    /// </summary>
    string? DataType { get; }
}

/// <summary>
/// Constraint that evaluates a Metapath expression that must return true.
/// </summary>
public interface IExpectConstraint : IConstraint
{
    /// <summary>
    /// Gets the Metapath expression that must evaluate to true.
    /// </summary>
    string Test { get; }
}

/// <summary>
/// Constraint that defines a named index for cross-reference validation.
/// </summary>
public interface IIndexConstraint : IConstraint
{
    /// <summary>
    /// Gets the name of the index.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the key fields that form the index key.
    /// </summary>
    IReadOnlyList<KeyField> KeyFields { get; }
}

/// <summary>
/// Constraint that validates a reference exists in a named index.
/// </summary>
public interface IIndexHasKeyConstraint : IConstraint
{
    /// <summary>
    /// Gets the name of the index to look up.
    /// </summary>
    string IndexName { get; }

    /// <summary>
    /// Gets the key fields that form the lookup key.
    /// </summary>
    IReadOnlyList<KeyField> KeyFields { get; }
}

/// <summary>
/// Constraint that ensures uniqueness within a scope.
/// </summary>
public interface IUniqueConstraint : IConstraint
{
    /// <summary>
    /// Gets the key fields that must be unique.
    /// </summary>
    IReadOnlyList<KeyField> KeyFields { get; }
}

/// <summary>
/// Constraint that validates minimum and maximum occurrence counts.
/// </summary>
public interface ICardinalityConstraint : IConstraint
{
    /// <summary>
    /// Gets the minimum number of occurrences required.
    /// </summary>
    int? MinOccurs { get; }

    /// <summary>
    /// Gets the maximum number of occurrences allowed.
    /// </summary>
    int? MaxOccurs { get; }
}

/// <summary>
/// Represents a key field in an index or uniqueness constraint.
/// </summary>
/// <param name="Target">Metapath expression to select the key value.</param>
/// <param name="Pattern">Optional regex pattern to extract part of the value.</param>
/// <param name="Remarks">Optional remarks about this key field.</param>
public readonly record struct KeyField(
    string Target,
    string? Pattern = null,
    string? Remarks = null);
