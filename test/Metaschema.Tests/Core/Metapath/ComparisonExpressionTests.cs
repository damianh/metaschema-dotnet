// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Metaschema.Core.Metapath.Context;
using Shouldly;
using Xunit;

namespace Metaschema.Core.Metapath;

/// <summary>
/// Tests for evaluating comparison and logical expressions.
/// </summary>
public class ComparisonExpressionTests
{
    private readonly IMetapathContext _context = MetapathContext.Create();

    private bool EvaluateBoolean(string expression)
    {
        var expr = MetapathExpression.Compile(expression);
        return expr.EvaluateBoolean(_context);
    }

    #region Value Comparison - eq (equals)

    [Theory]
    [InlineData("1 eq 1", true)]
    [InlineData("1 eq 2", false)]
    [InlineData("0 eq 0", true)]
    [InlineData("-5 eq -5", true)]
    [InlineData("-5 eq 5", false)]
    public void ValueComparison_Eq_Integers_ShouldEvaluate(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("'hello' eq 'hello'", true)]
    [InlineData("'hello' eq 'world'", false)]
    [InlineData("'' eq ''", true)]
    [InlineData("'abc' eq 'ABC'", false)] // Case-sensitive
    public void ValueComparison_Eq_Strings_ShouldEvaluate(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("true() eq true()", true)]
    [InlineData("false() eq false()", true)]
    [InlineData("true() eq false()", false)]
    public void ValueComparison_Eq_Booleans_ShouldEvaluate(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    #endregion

    #region Value Comparison - ne (not equals)

    [Theory]
    [InlineData("1 ne 2", true)]
    [InlineData("1 ne 1", false)]
    [InlineData("0 ne 1", true)]
    [InlineData("-1 ne 1", true)]
    public void ValueComparison_Ne_Integers_ShouldEvaluate(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("'hello' ne 'world'", true)]
    [InlineData("'hello' ne 'hello'", false)]
    public void ValueComparison_Ne_Strings_ShouldEvaluate(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    #endregion

    #region Value Comparison - lt (less than)

    [Theory]
    [InlineData("1 lt 2", true)]
    [InlineData("2 lt 1", false)]
    [InlineData("1 lt 1", false)]
    [InlineData("-5 lt 0", true)]
    [InlineData("-5 lt -10", false)]
    public void ValueComparison_Lt_Integers_ShouldEvaluate(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("'a' lt 'b'", true)]
    [InlineData("'b' lt 'a'", false)]
    [InlineData("'abc' lt 'abd'", true)]
    public void ValueComparison_Lt_Strings_ShouldEvaluate(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    #endregion

    #region Value Comparison - le (less than or equal)

    [Theory]
    [InlineData("1 le 2", true)]
    [InlineData("1 le 1", true)]
    [InlineData("2 le 1", false)]
    [InlineData("0 le 0", true)]
    public void ValueComparison_Le_Integers_ShouldEvaluate(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    #endregion

    #region Value Comparison - gt (greater than)

    [Theory]
    [InlineData("2 gt 1", true)]
    [InlineData("1 gt 2", false)]
    [InlineData("1 gt 1", false)]
    [InlineData("0 gt -5", true)]
    [InlineData("-5 gt -10", true)]
    public void ValueComparison_Gt_Integers_ShouldEvaluate(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("'b' gt 'a'", true)]
    [InlineData("'a' gt 'b'", false)]
    public void ValueComparison_Gt_Strings_ShouldEvaluate(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    #endregion

    #region Value Comparison - ge (greater than or equal)

    [Theory]
    [InlineData("2 ge 1", true)]
    [InlineData("1 ge 1", true)]
    [InlineData("1 ge 2", false)]
    [InlineData("0 ge 0", true)]
    public void ValueComparison_Ge_Integers_ShouldEvaluate(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    #endregion

    #region General Comparison - = (equals)

    [Theory]
    [InlineData("1 = 1", true)]
    [InlineData("1 = 2", false)]
    [InlineData("'test' = 'test'", true)]
    public void GeneralComparison_Equals_ShouldEvaluate(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    #endregion

    #region General Comparison - != (not equals)

    [Theory]
    [InlineData("1 != 2", true)]
    [InlineData("1 != 1", false)]
    [InlineData("'a' != 'b'", true)]
    public void GeneralComparison_NotEquals_ShouldEvaluate(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    #endregion

    #region General Comparison - < and >

    [Theory]
    [InlineData("1 < 2", true)]
    [InlineData("2 < 1", false)]
    [InlineData("2 > 1", true)]
    [InlineData("1 > 2", false)]
    [InlineData("1 <= 1", true)]
    [InlineData("1 <= 2", true)]
    [InlineData("1 >= 1", true)]
    [InlineData("2 >= 1", true)]
    public void GeneralComparison_Inequalities_ShouldEvaluate(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    #endregion

    #region Logical Operators - and

    [Theory]
    [InlineData("true() and true()", true)]
    [InlineData("true() and false()", false)]
    [InlineData("false() and true()", false)]
    [InlineData("false() and false()", false)]
    public void LogicalAnd_ShouldEvaluate(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("1 eq 1 and 2 eq 2", true)]
    [InlineData("1 eq 1 and 2 eq 3", false)]
    [InlineData("1 eq 2 and 2 eq 2", false)]
    public void LogicalAnd_WithComparisons_ShouldEvaluate(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    #endregion

    #region Logical Operators - or

    [Theory]
    [InlineData("true() or true()", true)]
    [InlineData("true() or false()", true)]
    [InlineData("false() or true()", true)]
    [InlineData("false() or false()", false)]
    public void LogicalOr_ShouldEvaluate(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("1 eq 1 or 2 eq 3", true)]
    [InlineData("1 eq 2 or 2 eq 3", false)]
    [InlineData("1 eq 2 or 2 eq 2", true)]
    public void LogicalOr_WithComparisons_ShouldEvaluate(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    #endregion

    #region Logical Operators - not()

    [Fact]
    public void LogicalNot_True_ShouldReturnFalse()
    {
        // Act
        bool result = EvaluateBoolean("not(true())");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void LogicalNot_False_ShouldReturnTrue()
    {
        // Act
        bool result = EvaluateBoolean("not(false())");

        // Assert
        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData("not(1 eq 1)", false)]
    [InlineData("not(1 eq 2)", true)]
    [InlineData("not(not(true()))", true)]
    public void LogicalNot_WithExpressions_ShouldEvaluate(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    #endregion

    #region Complex Logical Expressions

    [Theory]
    [InlineData("(1 eq 1) and (2 eq 2) and (3 eq 3)", true)]
    [InlineData("(1 eq 1) and ((2 eq 2) or (3 eq 4))", true)]
    [InlineData("((1 eq 2) or (2 eq 2)) and (3 eq 3)", true)]
    [InlineData("not(1 eq 2) and (2 eq 2)", true)]
    [InlineData("(1 gt 0) and (1 lt 10)", true)]
    [InlineData("(1 lt 0) or (1 gt 10)", false)]
    public void ComplexLogicalExpressions_ShouldEvaluate(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    #endregion

    #region Comparison with Arithmetic Results

    [Theory]
    [InlineData("1 + 1 eq 2", true)]
    [InlineData("2 * 3 eq 6", true)]
    [InlineData("10 - 5 eq 5", true)]
    [InlineData("(1 + 2) * 3 eq 9", true)]
    [InlineData("1 + 1 gt 1", true)]
    [InlineData("1 + 1 lt 3", true)]
    public void Comparison_WithArithmetic_ShouldEvaluate(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    #endregion
}
