// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using System.Text.RegularExpressions;

using Metaschema.Metapath;
using Metaschema.Metapath.Context;
using Metaschema.Metapath.Item;

namespace Metaschema.Constraints;

/// <summary>
/// Default implementation of constraint validation.
/// </summary>
public sealed class ConstraintValidator : IConstraintValidator
{
    private readonly Dictionary<string, Dictionary<string, INodeItem>> _indexes = new();
    private readonly IMetapathContext _baseContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConstraintValidator"/> class.
    /// </summary>
    public ConstraintValidator()
        : this(MetapathContext.Create())
    {
    }

    /// <summary>
    /// Initializes a new instance with a custom Metapath context.
    /// </summary>
    /// <param name="context">The Metapath context to use for evaluations.</param>
    public ConstraintValidator(IMetapathContext context) => _baseContext = context ?? throw new ArgumentNullException(nameof(context));

    /// <inheritdoc />
    public IEnumerable<ValidationFinding> Validate(INodeItem node, IConstraint constraint)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(constraint);

        // Evaluate target expression to find nodes to validate
        var targetNodes = GetTargetNodes(node, constraint.Target);

        foreach (var targetNode in targetNodes)
        {
            foreach (var finding in ValidateNode(targetNode, constraint))
            {
                yield return finding;
            }
        }
    }

    /// <inheritdoc />
    public ValidationResults ValidateAll(INodeItem root, IEnumerable<IConstraint> constraints)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(constraints);

        var results = new ValidationResults();

        // First pass: build indexes
        foreach (var constraint in constraints.OfType<IIndexConstraint>())
        {
            BuildIndex(root, constraint);
        }

        // Second pass: validate all constraints
        foreach (var constraint in constraints)
        {
            results.AddRange(Validate(root, constraint));
        }

        return results;
    }

    private IEnumerable<INodeItem> GetTargetNodes(INodeItem contextNode, string? target)
    {
        if (string.IsNullOrEmpty(target) || target == ".")
        {
            yield return contextNode;
            yield break;
        }

        var expr = MetapathExpression.Compile(target);
        var context = _baseContext.WithContextItem(contextNode);
        var result = expr.Evaluate(context);

        foreach (var item in result)
        {
            if (item is INodeItem node)
            {
                yield return node;
            }
        }
    }

    private IEnumerable<ValidationFinding> ValidateNode(INodeItem node, IConstraint constraint) => constraint switch
    {
        IAllowedValuesConstraint avc => ValidateAllowedValues(node, avc),
        IMatchesConstraint mc => ValidateMatches(node, mc),
        IExpectConstraint ec => ValidateExpect(node, ec),
        IIndexConstraint => [], // Indexes are built, not validated as findings
        IIndexHasKeyConstraint ihkc => ValidateIndexHasKey(node, ihkc),
        IUniqueConstraint uc => ValidateUnique(node, uc),
        ICardinalityConstraint cc => ValidateCardinality(node, cc),
        _ => []
    };

    private static IEnumerable<ValidationFinding> ValidateAllowedValues(INodeItem node, IAllowedValuesConstraint constraint)
    {
        var value = node.GetStringValue();
        if (value == null)
        {
            yield break;
        }

        var allowed = constraint.AllowedValues.Select(av => av.Value).ToHashSet(StringComparer.Ordinal);
        if (!allowed.Contains(value) && !constraint.AllowOther)
        {
            var allowedList = string.Join(", ", constraint.AllowedValues.Take(5).Select(av => $"'{av.Value}'"));
            if (constraint.AllowedValues.Count > 5)
            {
                allowedList += $" ... ({constraint.AllowedValues.Count} total)";
            }

            yield return ValidationFinding.Create(
                constraint,
                node,
                constraint.Message ?? $"Value '{value}' is not in allowed values: {allowedList}");
        }

        // Check for deprecated values
        var match = constraint.AllowedValues.FirstOrDefault(av => av.Value == value);
        if (match.DeprecatedVersion != null)
        {
            yield return new ValidationFinding(
                ConstraintLevel.Warning,
                node.GetPath(),
                constraint,
                $"Value '{value}' is deprecated since version {match.DeprecatedVersion}",
                node);
        }
    }

    private static List<ValidationFinding> ValidateMatches(INodeItem node, IMatchesConstraint constraint)
    {
        var findings = new List<ValidationFinding>();
        var value = node.GetStringValue();
        if (value == null)
        {
            return findings;
        }

        // Validate against pattern
        if (!string.IsNullOrEmpty(constraint.Pattern))
        {
            try
            {
                var regex = new Regex(constraint.Pattern, RegexOptions.None, TimeSpan.FromSeconds(1));
                if (!regex.IsMatch(value))
                {
                    findings.Add(ValidationFinding.Create(
                        constraint,
                        node,
                        constraint.Message ?? $"Value '{value}' does not match pattern '{constraint.Pattern}'"));
                }
            }
            catch (RegexParseException ex)
            {
                findings.Add(new ValidationFinding(
                    ConstraintLevel.Error,
                    node.GetPath(),
                    constraint,
                    $"Invalid regex pattern in constraint: {ex.Message}",
                    node));
            }
        }

        // Note: DataType validation would require the type system integration
        // For now, we focus on pattern matching
        return findings;
    }

    private List<ValidationFinding> ValidateExpect(INodeItem node, IExpectConstraint constraint)
    {
        var findings = new List<ValidationFinding>();
        try
        {
            var expr = MetapathExpression.Compile(constraint.Test);
            var context = _baseContext.WithContextItem(node);
            var result = expr.EvaluateBoolean(context);

            if (!result)
            {
                findings.Add(ValidationFinding.Create(
                    constraint,
                    node,
                    constraint.Message ?? $"Assertion failed: {constraint.Test}"));
            }
        }
        catch (MetapathException ex)
        {
            findings.Add(new ValidationFinding(
                ConstraintLevel.Error,
                node.GetPath(),
                constraint,
                $"Error evaluating expect constraint: {ex.Message}",
                node));
        }
        return findings;
    }

    private void BuildIndex(INodeItem root, IIndexConstraint constraint)
    {
        var indexEntries = new Dictionary<string, INodeItem>();
        var targetNodes = GetTargetNodes(root, constraint.Target);

        foreach (var node in targetNodes)
        {
            var key = ComputeKey(node, constraint.KeyFields);
            if (key != null)
            {
                indexEntries[key] = node; // Last one wins for duplicates
            }
        }

        _indexes[constraint.Name] = indexEntries;
    }

    private IEnumerable<ValidationFinding> ValidateIndexHasKey(INodeItem node, IIndexHasKeyConstraint constraint)
    {
        if (!_indexes.TryGetValue(constraint.IndexName, out var index))
        {
            yield return new ValidationFinding(
                ConstraintLevel.Error,
                node.GetPath(),
                constraint,
                $"Index '{constraint.IndexName}' not found. Ensure the index constraint is defined before this index-has-key constraint.",
                node);
            yield break;
        }

        var key = ComputeKey(node, constraint.KeyFields);
        if (key == null)
        {
            yield break; // Key fields not present
        }

        if (!index.ContainsKey(key))
        {
            yield return ValidationFinding.Create(
                constraint,
                node,
                constraint.Message ?? $"Key '{key}' not found in index '{constraint.IndexName}'");
        }
    }

    private static IEnumerable<ValidationFinding> ValidateUnique(INodeItem node, IUniqueConstraint constraint)
    {
        // Unique validation requires collecting all sibling nodes and checking for duplicates
        // This is typically done at the parent scope level
        // For now, we'll implement a simpler version that validates uniqueness during a full validation pass

        // Note: A more complete implementation would track seen keys per scope
        // and report findings when duplicates are encountered
        yield break;
    }

    private static IEnumerable<ValidationFinding> ValidateCardinality(INodeItem node, ICardinalityConstraint constraint)
    {
        // Get parent and count siblings with the same name
        var parent = node.Parent;
        if (parent == null)
        {
            yield break;
        }

        var siblings = parent.GetChildren()
            .Where(c => c.Name == node.Name)
            .ToList();

        var count = siblings.Count;

        if (constraint.MinOccurs.HasValue && count < constraint.MinOccurs.Value)
        {
            yield return ValidationFinding.Create(
                constraint,
                node,
                constraint.Message ?? $"Expected at least {constraint.MinOccurs} occurrences of '{node.Name}', found {count}");
        }

        // Only report max violation on the first extra occurrence
        if (constraint.MaxOccurs.HasValue && count > constraint.MaxOccurs.Value)
        {
            var index = siblings.IndexOf(node);
            if (index == constraint.MaxOccurs.Value) // First violation
            {
                yield return ValidationFinding.Create(
                    constraint,
                    node,
                    constraint.Message ?? $"Expected at most {constraint.MaxOccurs} occurrences of '{node.Name}', found {count}");
            }
        }
    }

    private string? ComputeKey(INodeItem node, IReadOnlyList<KeyField> keyFields)
    {
        var keyParts = new List<string>();

        foreach (var keyField in keyFields)
        {
            var fieldNodes = GetTargetNodes(node, keyField.Target).ToList();
            if (fieldNodes.Count == 0)
            {
                return null; // Key field not present
            }

            var value = fieldNodes[0].GetStringValue() ?? "";

            // Apply pattern extraction if specified
            if (!string.IsNullOrEmpty(keyField.Pattern))
            {
                try
                {
                    var regex = new Regex(keyField.Pattern, RegexOptions.None, TimeSpan.FromSeconds(1));
                    var match = regex.Match(value);
                    value = match.Success ? (match.Groups.Count > 1 ? match.Groups[1].Value : match.Value) : value;
                }
                catch (RegexParseException)
                {
                    // Use original value if pattern is invalid
                }
            }

            keyParts.Add(value);
        }

        return string.Join("|", keyParts);
    }

    /// <summary>
    /// Creates a new validator instance.
    /// </summary>
    public static ConstraintValidator Create() => new();
}
