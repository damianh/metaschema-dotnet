// Licensed under the MIT License.

using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Metaschema.Core.Constraints;

/// <summary>
/// Represents the results of constraint validation.
/// </summary>
[SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "ValidationResults is a domain-specific name")]
public sealed class ValidationResults : IReadOnlyCollection<ValidationFinding>
{
    private readonly List<ValidationFinding> _findings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationResults"/> class.
    /// </summary>
    public ValidationResults() => _findings = [];

    /// <summary>
    /// Initializes a new instance with the specified findings.
    /// </summary>
    /// <param name="findings">The initial findings.</param>
    public ValidationResults(IEnumerable<ValidationFinding> findings) =>
        _findings = [.. findings];

    /// <summary>
    /// Gets whether the validation passed (no critical or error findings).
    /// </summary>
    public bool IsValid => CriticalCount == 0 && ErrorCount == 0;

    /// <summary>
    /// Gets all findings.
    /// </summary>
    public IReadOnlyList<ValidationFinding> Findings => _findings;

    /// <summary>
    /// Gets the total number of findings.
    /// </summary>
    public int Count => _findings.Count;

    /// <summary>
    /// Gets the number of critical findings.
    /// </summary>
    public int CriticalCount => _findings.Count(f => f.Severity == ConstraintLevel.Critical);

    /// <summary>
    /// Gets the number of error findings.
    /// </summary>
    public int ErrorCount => _findings.Count(f => f.Severity == ConstraintLevel.Error);

    /// <summary>
    /// Gets the number of warning findings.
    /// </summary>
    public int WarningCount => _findings.Count(f => f.Severity == ConstraintLevel.Warning);

    /// <summary>
    /// Gets the number of informational findings.
    /// </summary>
    public int InformationalCount => _findings.Count(f => f.Severity == ConstraintLevel.Informational);

    /// <summary>
    /// Adds a finding to the results.
    /// </summary>
    /// <param name="finding">The finding to add.</param>
    public void Add(ValidationFinding finding) => _findings.Add(finding);

    /// <summary>
    /// Adds multiple findings to the results.
    /// </summary>
    /// <param name="findings">The findings to add.</param>
    public void AddRange(IEnumerable<ValidationFinding> findings) => _findings.AddRange(findings);

    /// <summary>
    /// Gets findings filtered by severity level.
    /// </summary>
    /// <param name="level">The severity level to filter by.</param>
    /// <returns>Findings with the specified severity.</returns>
    public IEnumerable<ValidationFinding> GetFindings(ConstraintLevel level) =>
        _findings.Where(f => f.Severity == level);

    /// <summary>
    /// Gets findings at or above the specified severity level.
    /// </summary>
    /// <param name="minimumLevel">The minimum severity level (lower values are more severe).</param>
    /// <returns>Findings at or above the specified severity.</returns>
    public IEnumerable<ValidationFinding> GetFindingsAtOrAbove(ConstraintLevel minimumLevel) =>
        _findings.Where(f => f.Severity <= minimumLevel);

    /// <summary>
    /// Merges findings from another results instance.
    /// </summary>
    /// <param name="other">The other results to merge.</param>
    public void Merge(ValidationResults other) => _findings.AddRange(other._findings);

    /// <inheritdoc />
    public IEnumerator<ValidationFinding> GetEnumerator() => _findings.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Returns a summary of the validation results.
    /// </summary>
    public override string ToString()
    {
        if (Count == 0)
            return "Validation passed: no findings";

        var status = IsValid ? "passed with warnings" : "failed";
        return $"Validation {status}: {CriticalCount} critical, {ErrorCount} errors, {WarningCount} warnings, {InformationalCount} info";
    }

    /// <summary>
    /// Creates an empty validation results instance.
    /// </summary>
    public static ValidationResults Empty { get; } = new();
}
