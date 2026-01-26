// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Metaschema.Metapath.Context;
using Metaschema.Metapath.Item;
using Shouldly;
using Xunit;

namespace Metaschema.Metapath;

/// <summary>
/// Tests for <see cref="MetapathExpression"/> parsing and basic evaluation.
/// </summary>
public class MetapathExpressionTests
{
    [Fact]
    public void Compile_WithValidExpression_ShouldSucceed()
    {
        // Act
        var expr = MetapathExpression.Compile("1 + 2");

        // Assert
        expr.ShouldNotBeNull();
        expr.Expression.ShouldBe("1 + 2");
    }

    [Fact]
    public void Compile_WithInvalidExpression_ShouldThrowMetapathException()
    {
        // Act & Assert - using truly invalid syntax
        Should.Throw<MetapathException>(() => MetapathExpression.Compile("1 + * 2"));
    }

    [Fact]
    public void Compile_WithEmptyString_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => MetapathExpression.Compile(""));
    }

    [Fact]
    public void Compile_WithWhitespaceOnly_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => MetapathExpression.Compile("   "));
    }

    [Fact]
    public void TryCompile_WithValidExpression_ShouldReturnTrue()
    {
        // Act
        var result = MetapathExpression.TryCompile("3 * 4", out var expr);

        // Assert
        result.ShouldBeTrue();
        expr.ShouldNotBeNull();
    }

    [Fact]
    public void TryCompile_WithInvalidExpression_ShouldReturnFalse()
    {
        // Act
        var result = MetapathExpression.TryCompile("invalid @ expression", out var expr);

        // Assert
        result.ShouldBeFalse();
        expr.ShouldBeNull();
    }

    [Fact]
    public void Evaluate_SimpleArithmetic_ShouldReturnResult()
    {
        // Arrange
        var expr = MetapathExpression.Compile("2 + 3");
        var context = MetapathContext.Create();

        // Act
        var result = expr.Evaluate(context);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        // The evaluator may return different numeric types
        var value = result.FirstOrDefault switch
        {
            IntegerItem i => i.Value,
            DecimalItem d => (long)d.Value,
            DoubleItem f => (long)f.Value,
            _ => throw new ShouldAssertException($"Expected numeric type")
        };
        value.ShouldBe(5);
    }

    [Fact]
    public void EvaluateSingle_WithSingleResult_ShouldReturnItem()
    {
        // Arrange
        var expr = MetapathExpression.Compile("42");
        var context = MetapathContext.Create();

        // Act
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldBeOfType<IntegerItem>();
        ((IntegerItem)result!).Value.ShouldBe(42);
    }

    [Fact]
    public void EvaluateBoolean_WithTrueExpression_ShouldReturnTrue()
    {
        // Arrange
        var expr = MetapathExpression.Compile("true()");
        var context = MetapathContext.Create();

        // Act
        var result = expr.EvaluateBoolean(context);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void EvaluateBoolean_WithFalseExpression_ShouldReturnFalse()
    {
        // Arrange
        var expr = MetapathExpression.Compile("false()");
        var context = MetapathContext.Create();

        // Act
        var result = expr.EvaluateBoolean(context);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void EvaluateString_WithStringLiteral_ShouldReturnString()
    {
        // Arrange
        var expr = MetapathExpression.Compile("'hello world'");
        var context = MetapathContext.Create();

        // Act
        var result = expr.EvaluateString(context);

        // Assert
        result.ShouldBe("hello world");
    }

    [Fact]
    public void ToString_ShouldReturnOriginalExpression()
    {
        // Arrange
        var expr = MetapathExpression.Compile("1 + 2 * 3");

        // Act & Assert
        expr.ToString().ShouldBe("1 + 2 * 3");
    }

    [Theory]
    [InlineData("1")]
    [InlineData("1 + 2")]
    [InlineData("(1 + 2) * 3")]
    [InlineData("'string literal'")]
    [InlineData("\"double quoted\"")]
    [InlineData("true()")]
    [InlineData("false()")]
    [InlineData("1.5")]
    [InlineData("-42")]
    [InlineData("1 eq 1")]
    [InlineData("1 ne 2")]
    [InlineData("1 lt 2")]
    [InlineData("2 gt 1")]
    [InlineData("1 le 1")]
    [InlineData("1 ge 1")]
    [InlineData("true() and true()")]
    [InlineData("true() or false()")]
    [InlineData("not(false())")]
    public void Compile_ValidExpressions_ShouldParse(string expression)
    {
        // Act
        var expr = MetapathExpression.Compile(expression);

        // Assert
        expr.ShouldNotBeNull();
        expr.Expression.ShouldBe(expression);
    }
}
