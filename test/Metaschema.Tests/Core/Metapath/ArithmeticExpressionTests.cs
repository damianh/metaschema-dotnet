// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Metaschema.Metapath.Context;
using Metaschema.Metapath.Item;
using Shouldly;
using Xunit;

namespace Metaschema.Metapath;

/// <summary>
/// Tests for evaluating arithmetic expressions.
/// </summary>
public class ArithmeticExpressionTests
{
    private readonly IMetapathContext _context = MetapathContext.Create();

    private long EvaluateInteger(string expression)
    {
        var expr = MetapathExpression.Compile(expression);
        var result = expr.EvaluateSingle(_context);
        result.ShouldNotBeNull();

        // The evaluator may return different numeric types
        return result switch
        {
            IntegerItem i => i.Value,
            DecimalItem d => (long)d.Value,
            DoubleItem f => (long)f.Value,
            _ => throw new ShouldAssertException($"Expected numeric type but got {result.GetType().Name}")
        };
    }

    private double EvaluateNumeric(string expression)
    {
        var expr = MetapathExpression.Compile(expression);
        var result = expr.EvaluateSingle(_context);
        result.ShouldNotBeNull();

        return result switch
        {
            IntegerItem i => i.Value,
            DecimalItem d => (double)d.Value,
            DoubleItem f => f.Value,
            _ => throw new ShouldAssertException($"Expected numeric type but got {result.GetType().Name}")
        };
    }

    #region Addition

    [Theory]
    [InlineData("1 + 1", 2)]
    [InlineData("0 + 0", 0)]
    [InlineData("10 + 20", 30)]
    [InlineData("100 + 200 + 300", 600)]
    [InlineData("-5 + 10", 5)]
    [InlineData("-5 + -5", -10)]
    public void Addition_Integers_ShouldEvaluate(string expression, long expected)
    {
        // Act
        var result = EvaluateInteger(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("1.5 + 1.5", 3.0)]
    [InlineData("0.1 + 0.2", 0.3)]
    [InlineData("1 + 0.5", 1.5)]
    public void Addition_Decimals_ShouldEvaluate(string expression, double expected)
    {
        // Act
        var result = EvaluateNumeric(expression);

        // Assert
        result.ShouldBe(expected, 0.00001);
    }

    #endregion

    #region Subtraction

    [Theory]
    [InlineData("5 - 3", 2)]
    [InlineData("10 - 10", 0)]
    [InlineData("100 - 50 - 25", 25)]
    [InlineData("0 - 5", -5)]
    [InlineData("-5 - 5", -10)]
    [InlineData("-5 - -3", -2)]
    public void Subtraction_Integers_ShouldEvaluate(string expression, long expected)
    {
        // Act
        var result = EvaluateInteger(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("3.5 - 1.5", 2.0)]
    [InlineData("1.0 - 0.5", 0.5)]
    public void Subtraction_Decimals_ShouldEvaluate(string expression, double expected)
    {
        // Act
        var result = EvaluateNumeric(expression);

        // Assert
        result.ShouldBe(expected, 0.00001);
    }

    #endregion

    #region Multiplication

    [Theory]
    [InlineData("2 * 3", 6)]
    [InlineData("0 * 100", 0)]
    [InlineData("1 * 42", 42)]
    [InlineData("10 * 10", 100)]
    [InlineData("-2 * 3", -6)]
    [InlineData("-2 * -3", 6)]
    [InlineData("2 * 3 * 4", 24)]
    public void Multiplication_Integers_ShouldEvaluate(string expression, long expected)
    {
        // Act
        var result = EvaluateInteger(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("2.0 * 3.0", 6.0)]
    [InlineData("1.5 * 2", 3.0)]
    [InlineData("0.5 * 0.5", 0.25)]
    public void Multiplication_Decimals_ShouldEvaluate(string expression, double expected)
    {
        // Act
        var result = EvaluateNumeric(expression);

        // Assert
        result.ShouldBe(expected, 0.00001);
    }

    #endregion

    #region Division

    [Theory]
    [InlineData("6 div 2", 3.0)]
    [InlineData("10 div 4", 2.5)]
    [InlineData("100 div 10", 10.0)]
    [InlineData("-6 div 2", -3.0)]
    [InlineData("6 div -2", -3.0)]
    [InlineData("-6 div -2", 3.0)]
    public void Division_ShouldEvaluate(string expression, double expected)
    {
        // Act
        var result = EvaluateNumeric(expression);

        // Assert
        result.ShouldBe(expected, 0.00001);
    }

    #endregion

    #region Integer Division

    [Theory]
    [InlineData("7 idiv 2", 3)]
    [InlineData("10 idiv 3", 3)]
    [InlineData("100 idiv 10", 10)]
    [InlineData("-7 idiv 2", -3)]
    [InlineData("7 idiv -2", -3)]
    public void IntegerDivision_ShouldEvaluate(string expression, long expected)
    {
        // Act
        var result = EvaluateInteger(expression);

        // Assert
        result.ShouldBe(expected);
    }

    #endregion

    #region Modulo

    [Theory]
    [InlineData("7 mod 3", 1)]
    [InlineData("10 mod 5", 0)]
    [InlineData("10 mod 3", 1)]
    [InlineData("100 mod 7", 2)]
    public void Modulo_ShouldEvaluate(string expression, long expected)
    {
        // Act
        var result = EvaluateInteger(expression);

        // Assert
        result.ShouldBe(expected);
    }

    #endregion

    #region Unary Operators

    [Theory]
    [InlineData("-5", -5)]
    [InlineData("--5", 5)]
    [InlineData("-(-5)", 5)]
    [InlineData("+5", 5)]
    [InlineData("+-5", -5)]
    public void UnaryOperators_ShouldEvaluate(string expression, long expected)
    {
        // Act
        var result = EvaluateInteger(expression);

        // Assert
        result.ShouldBe(expected);
    }

    #endregion

    #region Operator Precedence

    [Theory]
    [InlineData("2 + 3 * 4", 14)] // Multiplication before addition
    [InlineData("(2 + 3) * 4", 20)] // Parentheses first
    [InlineData("10 - 4 - 2", 4)] // Left-to-right
    [InlineData("10 - (4 - 2)", 8)] // Parentheses first
    [InlineData("2 * 3 + 4 * 5", 26)] // 6 + 20
    [InlineData("100 div 10 div 2", 5.0)] // Left-to-right
    public void OperatorPrecedence_ShouldBeCorrect(string expression, double expected)
    {
        // Act
        var result = EvaluateNumeric(expression);

        // Assert
        result.ShouldBe(expected, 0.00001);
    }

    [Theory]
    [InlineData("(1 + 2) * (3 + 4)", 21)]
    [InlineData("((2 + 3) * 4) + 1", 21)]
    [InlineData("2 * ((3 + 4) * 5)", 70)]
    public void NestedParentheses_ShouldEvaluateCorrectly(string expression, long expected)
    {
        // Act
        var result = EvaluateInteger(expression);

        // Assert
        result.ShouldBe(expected);
    }

    #endregion

    #region Complex Expressions

    [Theory]
    [InlineData("1 + 2 + 3 + 4 + 5", 15)]
    [InlineData("10 * 10 + 5 * 5", 125)]
    [InlineData("(10 + 5) * (10 - 5)", 75)]
    public void ComplexExpressions_ShouldEvaluate(string expression, long expected)
    {
        // Act
        var result = EvaluateInteger(expression);

        // Assert
        result.ShouldBe(expected);
    }

    #endregion
}
