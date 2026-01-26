// Licensed under the MIT License.

using Metaschema.Metapath.Item;
using Shouldly;
using Xunit;

namespace Metaschema.Constraints;

/// <summary>
/// Tests for constraint validation.
/// </summary>
public class ConstraintValidatorTests
{
    #region AllowedValues Constraint Tests

    [Fact]
    public void AllowedValuesConstraint_WithValidValue_ShouldPass()
    {
        // Arrange
        var constraint = new AllowedValuesConstraint
        {
            AllowedValues = [new AllowedValue("high"), new AllowedValue("medium"), new AllowedValue("low")]
        };
        var node = new TestNodeItem { StringValue = "high" };
        var validator = ConstraintValidator.Create();

        // Act
        var findings = validator.Validate(node, constraint).ToList();

        // Assert
        findings.ShouldBeEmpty();
    }

    [Fact]
    public void AllowedValuesConstraint_WithInvalidValue_ShouldFail()
    {
        // Arrange
        var constraint = new AllowedValuesConstraint
        {
            AllowedValues = [new AllowedValue("high"), new AllowedValue("medium"), new AllowedValue("low")]
        };
        var node = new TestNodeItem { StringValue = "critical" };
        var validator = ConstraintValidator.Create();

        // Act
        var findings = validator.Validate(node, constraint).ToList();

        // Assert
        findings.Count.ShouldBe(1);
        findings[0].Severity.ShouldBe(ConstraintLevel.Error);
        findings[0].Message.ShouldContain("critical");
        findings[0].Message.ShouldContain("allowed values");
    }

    [Fact]
    public void AllowedValuesConstraint_WithAllowOther_ShouldPassForUnlistedValue()
    {
        // Arrange
        var constraint = new AllowedValuesConstraint
        {
            AllowedValues = [new AllowedValue("high"), new AllowedValue("low")],
            AllowOther = true
        };
        var node = new TestNodeItem { StringValue = "custom" };
        var validator = ConstraintValidator.Create();

        // Act
        var findings = validator.Validate(node, constraint).ToList();

        // Assert
        findings.ShouldBeEmpty();
    }

    [Fact]
    public void AllowedValuesConstraint_WithDeprecatedValue_ShouldWarn()
    {
        // Arrange
        var constraint = new AllowedValuesConstraint
        {
            AllowedValues = [
                new AllowedValue("current"),
                new AllowedValue("legacy", null, "1.0.0")
            ]
        };
        var node = new TestNodeItem { StringValue = "legacy" };
        var validator = ConstraintValidator.Create();

        // Act
        var findings = validator.Validate(node, constraint).ToList();

        // Assert
        findings.Count.ShouldBe(1);
        findings[0].Severity.ShouldBe(ConstraintLevel.Warning);
        findings[0].Message.ShouldContain("deprecated");
        findings[0].Message.ShouldContain("1.0.0");
    }

    #endregion

    #region Matches Constraint Tests

    [Fact]
    public void MatchesConstraint_WithMatchingPattern_ShouldPass()
    {
        // Arrange
        var constraint = new MatchesConstraint
        {
            Pattern = @"^[a-z]+$"
        };
        var node = new TestNodeItem { StringValue = "hello" };
        var validator = ConstraintValidator.Create();

        // Act
        var findings = validator.Validate(node, constraint).ToList();

        // Assert
        findings.ShouldBeEmpty();
    }

    [Fact]
    public void MatchesConstraint_WithNonMatchingPattern_ShouldFail()
    {
        // Arrange
        var constraint = new MatchesConstraint
        {
            Pattern = @"^[a-z]+$"
        };
        var node = new TestNodeItem { StringValue = "Hello123" };
        var validator = ConstraintValidator.Create();

        // Act
        var findings = validator.Validate(node, constraint).ToList();

        // Assert
        findings.Count.ShouldBe(1);
        findings[0].Severity.ShouldBe(ConstraintLevel.Error);
        findings[0].Message.ShouldContain("Hello123");
        findings[0].Message.ShouldContain("pattern");
    }

    [Fact]
    public void MatchesConstraint_WithInvalidRegex_ShouldReportError()
    {
        // Arrange
        var constraint = new MatchesConstraint
        {
            Pattern = @"[invalid(" // Invalid regex
        };
        var node = new TestNodeItem { StringValue = "test" };
        var validator = ConstraintValidator.Create();

        // Act
        var findings = validator.Validate(node, constraint).ToList();

        // Assert
        findings.Count.ShouldBe(1);
        findings[0].Message.ShouldContain("Invalid regex pattern");
    }

    #endregion

    #region Expect Constraint Tests

    [Fact]
    public void ExpectConstraint_WithTrueExpression_ShouldPass()
    {
        // Arrange
        var constraint = new ExpectConstraint
        {
            Test = "true()"
        };
        var node = new TestNodeItem { StringValue = "test" };
        var validator = ConstraintValidator.Create();

        // Act
        var findings = validator.Validate(node, constraint).ToList();

        // Assert
        findings.ShouldBeEmpty();
    }

    [Fact]
    public void ExpectConstraint_WithFalseExpression_ShouldFail()
    {
        // Arrange
        var constraint = new ExpectConstraint
        {
            Test = "false()"
        };
        var node = new TestNodeItem { StringValue = "test" };
        var validator = ConstraintValidator.Create();

        // Act
        var findings = validator.Validate(node, constraint).ToList();

        // Assert
        findings.Count.ShouldBe(1);
        findings[0].Severity.ShouldBe(ConstraintLevel.Error);
        findings[0].Message.ShouldContain("Assertion failed");
    }

    [Fact]
    public void ExpectConstraint_WithCustomMessage_ShouldUseCustomMessage()
    {
        // Arrange
        var constraint = new ExpectConstraint
        {
            Test = "false()",
            Message = "Custom validation message"
        };
        var node = new TestNodeItem { StringValue = "test" };
        var validator = ConstraintValidator.Create();

        // Act
        var findings = validator.Validate(node, constraint).ToList();

        // Assert
        findings.Count.ShouldBe(1);
        findings[0].Message.ShouldBe("Custom validation message");
    }

    [Fact]
    public void ExpectConstraint_WithInvalidExpression_ShouldReportError()
    {
        // Arrange
        var constraint = new ExpectConstraint
        {
            Test = "invalid-function()"
        };
        var node = new TestNodeItem { StringValue = "test" };
        var validator = ConstraintValidator.Create();

        // Act
        var findings = validator.Validate(node, constraint).ToList();

        // Assert
        findings.Count.ShouldBe(1);
        findings[0].Message.ShouldContain("Error evaluating");
    }

    #endregion

    #region Cardinality Constraint Tests

    [Fact]
    public void CardinalityConstraint_WithMinOccursViolation_ShouldFail()
    {
        // Arrange
        var constraint = new CardinalityConstraint
        {
            MinOccurs = 2
        };
        var parent = new TestNodeItem { Name = "parent" };
        var child = new TestNodeItem { Name = "child", ParentNode = parent };
        parent.ChildNodes = [child]; // Only 1 child, but min is 2

        var validator = ConstraintValidator.Create();

        // Act
        var findings = validator.Validate(child, constraint).ToList();

        // Assert
        findings.Count.ShouldBe(1);
        findings[0].Message.ShouldContain("at least 2");
    }

    #endregion

    #region ValidationResults Tests

    [Fact]
    public void ValidationResults_Empty_ShouldBeValid()
    {
        // Arrange
        var results = new ValidationResults();

        // Assert
        results.IsValid.ShouldBeTrue();
        results.Count.ShouldBe(0);
    }

    [Fact]
    public void ValidationResults_WithError_ShouldBeInvalid()
    {
        // Arrange
        var results = new ValidationResults();
        var constraint = new ExpectConstraint { Test = "true()" };
        results.Add(new ValidationFinding(ConstraintLevel.Error, "/path", constraint, "Error message"));

        // Assert
        results.IsValid.ShouldBeFalse();
        results.ErrorCount.ShouldBe(1);
    }

    [Fact]
    public void ValidationResults_WithOnlyWarnings_ShouldBeValid()
    {
        // Arrange
        var results = new ValidationResults();
        var constraint = new ExpectConstraint { Test = "true()" };
        results.Add(new ValidationFinding(ConstraintLevel.Warning, "/path", constraint, "Warning message"));

        // Assert
        results.IsValid.ShouldBeTrue();
        results.WarningCount.ShouldBe(1);
    }

    [Fact]
    public void ValidationResults_Merge_ShouldCombineFindings()
    {
        // Arrange
        var results1 = new ValidationResults();
        var results2 = new ValidationResults();
        var constraint = new ExpectConstraint { Test = "true()" };

        results1.Add(new ValidationFinding(ConstraintLevel.Error, "/path1", constraint, "Error 1"));
        results2.Add(new ValidationFinding(ConstraintLevel.Warning, "/path2", constraint, "Warning 1"));

        // Act
        results1.Merge(results2);

        // Assert
        results1.Count.ShouldBe(2);
        results1.ErrorCount.ShouldBe(1);
        results1.WarningCount.ShouldBe(1);
    }

    #endregion

    #region Constraint Level Tests

    [Theory]
    [InlineData(ConstraintLevel.Critical)]
    [InlineData(ConstraintLevel.Error)]
    [InlineData(ConstraintLevel.Warning)]
    [InlineData(ConstraintLevel.Informational)]
    public void ConstraintLevel_ShouldBePreservedInFinding(ConstraintLevel level)
    {
        // Arrange
        var constraint = new AllowedValuesConstraint
        {
            Level = level,
            AllowedValues = [new AllowedValue("valid")]
        };
        var node = new TestNodeItem { StringValue = "invalid" };
        var validator = ConstraintValidator.Create();

        // Act
        var findings = validator.Validate(node, constraint).ToList();

        // Assert
        findings.Count.ShouldBe(1);
        findings[0].Severity.ShouldBe(level);
    }

    #endregion

    #region ValidateAll Tests

    [Fact]
    public void ValidateAll_WithMultipleConstraints_ShouldValidateAll()
    {
        // Arrange
        var constraints = new List<IConstraint>
        {
            new AllowedValuesConstraint
            {
                AllowedValues = [new AllowedValue("valid")]
            },
            new MatchesConstraint
            {
                Pattern = @"^[a-z]+$"
            }
        };
        var node = new TestNodeItem { StringValue = "INVALID" }; // Fails both constraints
        var validator = ConstraintValidator.Create();

        // Act
        var results = validator.ValidateAll(node, constraints);

        // Assert
        results.Count.ShouldBe(2);
        results.IsValid.ShouldBeFalse();
    }

    #endregion

    #region Test Helpers

    /// <summary>
    /// A simple test implementation of INodeItem for testing purposes.
    /// </summary>
    private sealed class TestNodeItem : INodeItem
    {
        public NodeType NodeType => NodeType.Field;
        public string? Name { get; init; } = "test";
        public INodeItem? Parent => ParentNode;
        public TestNodeItem? ParentNode { get; init; }
        public Uri? BaseUri => null;
        public Uri? DocumentUri => null;
        public string? NamespaceUri => null;
        public object? Definition => null;
        public string? StringValue { get; init; }
        public List<TestNodeItem> ChildNodes { get; set; } = [];

        public IEnumerable<INodeItem> GetChildren() => ChildNodes;
        public IEnumerable<INodeItem> GetFlags() => [];
        public IEnumerable<INodeItem> GetModelItems() => ChildNodes;
        public INodeItem? GetFlag(string name) => null;

        public string GetPath()
        {
            var parts = new List<string>();
            INodeItem? current = this;
            while (current is not null)
            {
                if (current.Name is not null)
                    parts.Add(current.Name);
                current = current.Parent;
            }
            parts.Reverse();
            return "/" + string.Join("/", parts);
        }

        public string GetStringValue() => StringValue ?? string.Empty;
        public object? GetTypedValue() => StringValue;
        public bool GetEffectiveBooleanValue() => !string.IsNullOrEmpty(StringValue);
    }

    #endregion
}
