// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Metaschema.Core.Metapath.Context;
using Metaschema.Core.Metapath.Item;
using Shouldly;
using Xunit;

namespace Metaschema.Core.Metapath;

/// <summary>
/// Tests for evaluating literal expressions (strings, numbers, booleans).
/// </summary>
public class LiteralEvaluationTests
{
    private readonly IMetapathContext _context = MetapathContext.Create();

    private IItem? Evaluate(string expression)
    {
        var expr = MetapathExpression.Compile(expression);
        return expr.EvaluateSingle(_context);
    }

    private T EvaluateAs<T>(string expression) where T : IItem
    {
        var result = Evaluate(expression);
        result.ShouldNotBeNull();
        result.ShouldBeOfType<T>();
        return (T)result;
    }

    #region Integer Literals

    [Theory]
    [InlineData("0", 0)]
    [InlineData("1", 1)]
    [InlineData("42", 42)]
    [InlineData("123456789", 123456789)]
    [InlineData("9223372036854775807", long.MaxValue)] // Max long value
    public void IntegerLiteral_ShouldEvaluate(string expression, long expected)
    {
        // Act
        var result = EvaluateAs<IntegerItem>(expression);

        // Assert
        result.Value.ShouldBe(expected);
    }

    [Theory]
    [InlineData("-1", -1)]
    [InlineData("-42", -42)]
    [InlineData("-9223372036854775807", long.MinValue + 1)] // Near min long value
    public void NegativeIntegerLiteral_ShouldEvaluate(string expression, long expected)
    {
        // Act
        var result = EvaluateAs<IntegerItem>(expression);

        // Assert
        result.Value.ShouldBe(expected);
    }

    #endregion

    #region Decimal Literals

    [Theory]
    [InlineData("0.0", 0.0)]
    [InlineData("1.0", 1.0)]
    [InlineData("3.14159", 3.14159)]
    [InlineData("0.5", 0.5)]
    [InlineData("123.456", 123.456)]
    public void DecimalLiteral_ShouldEvaluate(string expression, double expected)
    {
        // Act
        var result = Evaluate(expression);

        // Assert - could be decimal or double depending on implementation
        result.ShouldNotBeNull();
        if (result is DecimalItem decimalItem)
        {
            ((double)decimalItem.Value).ShouldBe(expected, 0.00001);
        }
        else if (result is DoubleItem doubleItem)
        {
            doubleItem.Value.ShouldBe(expected, 0.00001);
        }
        else
        {
            throw new ShouldAssertException($"Expected DecimalItem or DoubleItem but got {result.GetType().Name}");
        }
    }

    [Theory]
    [InlineData("-1.5", -1.5)]
    [InlineData("-3.14", -3.14)]
    public void NegativeDecimalLiteral_ShouldEvaluate(string expression, double expected)
    {
        // Act
        var result = Evaluate(expression);

        // Assert
        result.ShouldNotBeNull();
        if (result is DecimalItem decimalItem)
        {
            ((double)decimalItem.Value).ShouldBe(expected, 0.00001);
        }
        else if (result is DoubleItem doubleItem)
        {
            doubleItem.Value.ShouldBe(expected, 0.00001);
        }
        else
        {
            throw new ShouldAssertException($"Expected DecimalItem or DoubleItem but got {result.GetType().Name}");
        }
    }

    #endregion

    #region String Literals

    [Fact]
    public void SingleQuotedString_ShouldEvaluate()
    {
        // Act
        var result = EvaluateAs<StringItem>("'hello'");

        // Assert
        result.Value.ShouldBe("hello");
    }

    [Fact]
    public void DoubleQuotedString_ShouldEvaluate()
    {
        // Act
        var result = EvaluateAs<StringItem>("\"hello\"");

        // Assert
        result.Value.ShouldBe("hello");
    }

    [Fact]
    public void EmptyString_SingleQuoted_ShouldEvaluate()
    {
        // Act
        var result = EvaluateAs<StringItem>("''");

        // Assert
        result.Value.ShouldBe("");
    }

    [Fact]
    public void EmptyString_DoubleQuoted_ShouldEvaluate()
    {
        // Act
        var result = EvaluateAs<StringItem>("\"\"");

        // Assert
        result.Value.ShouldBe("");
    }

    [Fact]
    public void StringWithSpaces_ShouldEvaluate()
    {
        // Act
        var result = EvaluateAs<StringItem>("'hello world'");

        // Assert
        result.Value.ShouldBe("hello world");
    }

    [Fact]
    public void StringWithNumbers_ShouldEvaluate()
    {
        // Act
        var result = EvaluateAs<StringItem>("'abc123'");

        // Assert
        result.Value.ShouldBe("abc123");
    }

    [Fact]
    public void StringWithSpecialCharacters_ShouldEvaluate()
    {
        // Act
        var result = EvaluateAs<StringItem>("'hello-world_test.txt'");

        // Assert
        result.Value.ShouldBe("hello-world_test.txt");
    }

    [Fact]
    public void StringWithEscapedSingleQuote_ShouldEvaluate()
    {
        // In XPath/Metapath, single quotes are escaped by doubling them
        // Act
        var result = EvaluateAs<StringItem>("'it''s'");

        // Assert
        result.Value.ShouldBe("it's");
    }

    [Fact]
    public void StringWithEscapedDoubleQuote_ShouldEvaluate()
    {
        // In XPath/Metapath, double quotes are escaped by doubling them
        // Act
        var result = EvaluateAs<StringItem>("\"say \"\"hello\"\"\"");

        // Assert
        result.Value.ShouldBe("say \"hello\"");
    }

    #endregion

    #region Boolean Literals (via functions)

    [Fact]
    public void TrueFunction_ShouldEvaluate()
    {
        // Act
        var result = EvaluateAs<BooleanItem>("true()");

        // Assert
        result.Value.ShouldBeTrue();
    }

    [Fact]
    public void FalseFunction_ShouldEvaluate()
    {
        // Act
        var result = EvaluateAs<BooleanItem>("false()");

        // Assert
        result.Value.ShouldBeFalse();
    }

    #endregion

    #region Effective Boolean Value

    [Theory]
    [InlineData("0", false)]
    [InlineData("1", true)]
    [InlineData("-1", true)]
    [InlineData("42", true)]
    public void IntegerEffectiveBooleanValue_ShouldBeCorrect(string expression, bool expected)
    {
        // Arrange
        var expr = MetapathExpression.Compile(expression);

        // Act
        var result = expr.EvaluateBoolean(_context);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("''", false)]
    [InlineData("'a'", true)]
    [InlineData("'hello'", true)]
    public void StringEffectiveBooleanValue_ShouldBeCorrect(string expression, bool expected)
    {
        // Arrange
        var expr = MetapathExpression.Compile(expression);

        // Act
        var result = expr.EvaluateBoolean(_context);

        // Assert
        result.ShouldBe(expected);
    }

    #endregion

    #region GetStringValue

    [Fact]
    public void IntegerItem_GetStringValue_ShouldReturnString()
    {
        // Arrange
        var item = new IntegerItem(42);

        // Act & Assert
        item.GetStringValue().ShouldBe("42");
    }

    [Fact]
    public void BooleanItem_GetStringValue_ShouldReturnString()
    {
        // Act & Assert
        BooleanItem.True.GetStringValue().ShouldBe("true");
        BooleanItem.False.GetStringValue().ShouldBe("false");
    }

    [Fact]
    public void DecimalItem_GetStringValue_ShouldReturnString()
    {
        // Arrange
        var item = new DecimalItem(3.14m);

        // Act & Assert
        item.GetStringValue().ShouldBe("3.14");
    }

    #endregion
}
