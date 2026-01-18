// Licensed under the MIT License.

namespace Metaschema.Core.Constraints;

/// <summary>
/// Base class for all constraint implementations.
/// </summary>
public abstract class ConstraintBase : IConstraint
{
    /// <inheritdoc />
    public string? Id { get; init; }

    /// <inheritdoc />
    public ConstraintLevel Level { get; init; } = ConstraintLevel.Error;

    /// <inheritdoc />
    public string? Target { get; init; }

    /// <inheritdoc />
    public string? Message { get; init; }

    /// <inheritdoc />
    public string? Remarks { get; init; }

    /// <inheritdoc />
    public ConstraintSource Source { get; init; } = ConstraintSource.Model;
}

/// <summary>
/// Constraint that restricts values to an enumerated set.
/// </summary>
public sealed class AllowedValuesConstraint : ConstraintBase, IAllowedValuesConstraint
{
    /// <inheritdoc />
    public IReadOnlyList<AllowedValue> AllowedValues { get; init; } = [];

    /// <inheritdoc />
    public bool AllowOther { get; init; }

    /// <inheritdoc />
    public bool Extensible { get; init; } = true;
}

/// <summary>
/// Constraint that validates values against a regular expression pattern.
/// </summary>
public sealed class MatchesConstraint : ConstraintBase, IMatchesConstraint
{
    /// <inheritdoc />
    public string? Pattern { get; init; }

    /// <inheritdoc />
    public string? DataType { get; init; }
}

/// <summary>
/// Constraint that evaluates a Metapath expression that must return true.
/// </summary>
public sealed class ExpectConstraint : ConstraintBase, IExpectConstraint
{
    /// <inheritdoc />
    public required string Test { get; init; }
}

/// <summary>
/// Constraint that defines a named index for cross-reference validation.
/// </summary>
public sealed class IndexConstraint : ConstraintBase, IIndexConstraint
{
    /// <inheritdoc />
    public required string Name { get; init; }

    /// <inheritdoc />
    public IReadOnlyList<KeyField> KeyFields { get; init; } = [];
}

/// <summary>
/// Constraint that validates a reference exists in a named index.
/// </summary>
public sealed class IndexHasKeyConstraint : ConstraintBase, IIndexHasKeyConstraint
{
    /// <inheritdoc />
    public required string IndexName { get; init; }

    /// <inheritdoc />
    public IReadOnlyList<KeyField> KeyFields { get; init; } = [];
}

/// <summary>
/// Constraint that ensures uniqueness within a scope.
/// </summary>
public sealed class UniqueConstraint : ConstraintBase, IUniqueConstraint
{
    /// <inheritdoc />
    public IReadOnlyList<KeyField> KeyFields { get; init; } = [];
}

/// <summary>
/// Constraint that validates minimum and maximum occurrence counts.
/// </summary>
public sealed class CardinalityConstraint : ConstraintBase, ICardinalityConstraint
{
    /// <inheritdoc />
    public int? MinOccurs { get; init; }

    /// <inheritdoc />
    public int? MaxOccurs { get; init; }
}
