// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Metaschema.Core.Metapath.Context;
using Metaschema.Core.Metapath.Item;
using Shouldly;
using Xunit;

namespace Metaschema.Core.Metapath;

/// <summary>
/// Tests for evaluating built-in function calls.
/// </summary>
public class FunctionCallTests
{
    private readonly IMetapathContext _context = MetapathContext.Create();

    private IItem? EvaluateSingle(string expression)
    {
        var expr = MetapathExpression.Compile(expression);
        return expr.EvaluateSingle(_context);
    }

    private ISequence Evaluate(string expression)
    {
        var expr = MetapathExpression.Compile(expression);
        return expr.Evaluate(_context);
    }

    private bool EvaluateBoolean(string expression)
    {
        var expr = MetapathExpression.Compile(expression);
        return expr.EvaluateBoolean(_context);
    }

    private string? EvaluateString(string expression)
    {
        var expr = MetapathExpression.Compile(expression);
        return expr.EvaluateString(_context);
    }

    private long EvaluateInteger(string expression)
    {
        var result = EvaluateSingle(expression);
        result.ShouldNotBeNull();
        result.ShouldBeOfType<IntegerItem>();
        return ((IntegerItem)result).Value;
    }

    private double EvaluateNumeric(string expression)
    {
        var result = EvaluateSingle(expression);
        result.ShouldNotBeNull();
        return result switch
        {
            IntegerItem i => i.Value,
            DecimalItem d => (double)d.Value,
            DoubleItem f => f.Value,
            _ => throw new ShouldAssertException($"Expected numeric type but got {result.GetType().Name}")
        };
    }

    #region Context Functions

    [Fact]
    public void PositionFunction_ShouldReturnContextPosition()
    {
        // Arrange - create context with position 3 of 5
        var dynamicContext = new DynamicContext().WithPosition(3, 5);
        var context = new MetapathContext(StaticContext.CreateDefault(), dynamicContext);

        // Act
        var expr = MetapathExpression.Compile("position()");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<IntegerItem>();
        ((IntegerItem)result).Value.ShouldBe(3);
    }

    [Fact]
    public void LastFunction_ShouldReturnContextSize()
    {
        // Arrange - create context with position 3 of 5
        var dynamicContext = new DynamicContext().WithPosition(3, 5);
        var context = new MetapathContext(StaticContext.CreateDefault(), dynamicContext);

        // Act
        var expr = MetapathExpression.Compile("last()");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<IntegerItem>();
        ((IntegerItem)result).Value.ShouldBe(5);
    }

    #endregion

    #region Boolean Functions

    [Fact]
    public void TrueFunction_ShouldReturnTrue()
    {
        // Act
        bool result = EvaluateBoolean("true()");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void FalseFunction_ShouldReturnFalse()
    {
        // Act
        bool result = EvaluateBoolean("false()");

        // Assert
        result.ShouldBeFalse();
    }

    [Theory]
    [InlineData("not(true())", false)]
    [InlineData("not(false())", true)]
    [InlineData("not(1 eq 1)", false)]
    [InlineData("not(1 eq 2)", true)]
    public void NotFunction_ShouldNegate(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("boolean(true())", true)]
    [InlineData("boolean(false())", false)]
    [InlineData("boolean(1)", true)]
    [InlineData("boolean(0)", false)]
    [InlineData("boolean('hello')", true)]
    [InlineData("boolean('')", false)]
    public void BooleanFunction_ShouldConvert(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    #endregion

    #region Sequence Functions

    [Theory]
    [InlineData("empty(())", true)]
    [InlineData("empty((1))", false)]
    [InlineData("empty((1, 2, 3))", false)]
    public void EmptyFunction_ShouldCheckIfEmpty(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("exists(())", false)]
    [InlineData("exists(1)", true)]
    [InlineData("exists('hello')", true)]
    public void ExistsFunction_ShouldCheckIfExists(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("count(())", 0)]
    [InlineData("count((1, 2, 3))", 3)]
    public void CountFunction_ShouldReturnCount(string expression, long expected)
    {
        // Act
        long result = EvaluateInteger(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void HeadFunction_ShouldReturnFirstItem()
    {
        // Act
        long result = EvaluateInteger("head((1, 2, 3))");

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public void TailFunction_ShouldReturnRemainingItems()
    {
        // Act
        var result = Evaluate("tail((1, 2, 3))");

        // Assert
        result.Count.ShouldBe(2);
        ((IntegerItem)result.ToList()[0]).Value.ShouldBe(2);
        ((IntegerItem)result.ToList()[1]).Value.ShouldBe(3);
    }

    #endregion

    #region String Functions

    [Theory]
    [InlineData("string(123)", "123")]
    [InlineData("string(true())", "true")]
    [InlineData("string(false())", "false")]
    [InlineData("string('hello')", "hello")]
    public void StringFunction_ShouldConvert(string expression, string expected)
    {
        // Act
        string? result = EvaluateString(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("concat('hello', ' ', 'world')", "hello world")]
    [InlineData("concat('a', 'b', 'c')", "abc")]
    [InlineData("concat('test')", "test")]
    [InlineData("concat('', '')", "")]
    public void ConcatFunction_ShouldConcatenate(string expression, string expected)
    {
        // Act
        string? result = EvaluateString(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("string-length('')", 0)]
    [InlineData("string-length('hello')", 5)]
    [InlineData("string-length('hello world')", 11)]
    public void StringLengthFunction_ShouldReturnLength(string expression, long expected)
    {
        // Act
        long result = EvaluateInteger(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("normalize-space('  hello  ')", "hello")]
    [InlineData("normalize-space('  hello   world  ')", "hello world")]
    [InlineData("normalize-space('test')", "test")]
    [InlineData("normalize-space('   ')", "")]
    public void NormalizeSpaceFunction_ShouldNormalize(string expression, string expected)
    {
        // Act
        string? result = EvaluateString(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("contains('hello world', 'world')", true)]
    [InlineData("contains('hello world', 'foo')", false)]
    [InlineData("contains('test', '')", true)]
    [InlineData("contains('', 'test')", false)]
    public void ContainsFunction_ShouldCheckContainment(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("starts-with('hello world', 'hello')", true)]
    [InlineData("starts-with('hello world', 'world')", false)]
    [InlineData("starts-with('test', '')", true)]
    [InlineData("starts-with('test', 'test')", true)]
    public void StartsWithFunction_ShouldCheck(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("ends-with('hello world', 'world')", true)]
    [InlineData("ends-with('hello world', 'hello')", false)]
    [InlineData("ends-with('test', '')", true)]
    [InlineData("ends-with('test', 'test')", true)]
    public void EndsWithFunction_ShouldCheck(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("substring('hello', 2)", "ello")]
    [InlineData("substring('hello', 1)", "hello")]
    [InlineData("substring('hello', 2, 3)", "ell")]
    [InlineData("substring('hello', 1, 2)", "he")]
    public void SubstringFunction_ShouldExtract(string expression, string expected)
    {
        // Act
        string? result = EvaluateString(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("upper-case('hello')", "HELLO")]
    [InlineData("upper-case('Hello World')", "HELLO WORLD")]
    [InlineData("upper-case('')", "")]
    public void UpperCaseFunction_ShouldConvert(string expression, string expected)
    {
        // Act
        string? result = EvaluateString(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("lower-case('HELLO')", "hello")]
    [InlineData("lower-case('Hello World')", "hello world")]
    [InlineData("lower-case('')", "")]
    public void LowerCaseFunction_ShouldConvert(string expression, string expected)
    {
        // Act
        string? result = EvaluateString(expression);

        // Assert
        result.ShouldBe(expected);
    }

    #endregion

    #region Numeric Functions

    [Theory]
    [InlineData("abs(-5)", 5.0)]
    [InlineData("abs(0)", 0.0)]
    public void AbsFunction_ShouldReturnAbsoluteValue(string expression, double expected)
    {
        // Act
        double result = EvaluateNumeric(expression);

        // Assert
        result.ShouldBe(expected, 0.00001);
    }

    [Theory]
    [InlineData("round(1.5)", 2.0)]
    [InlineData("round(1.4)", 1.0)]
    [InlineData("round(-1.5)", -2.0)] // Math.Round uses banker's rounding (MidpointRounding.ToEven)
    [InlineData("round(2.5)", 2.0)]   // 2.5 rounds to 2 (nearest even) with banker's rounding
    public void RoundFunction_ShouldRound(string expression, double expected)
    {
        // Act
        double result = EvaluateNumeric(expression);

        // Assert
        result.ShouldBe(expected, 0.00001);
    }

    [Theory]
    [InlineData("floor(1.9)", 1.0)]
    [InlineData("floor(1.1)", 1.0)]
    [InlineData("floor(-1.1)", -2.0)]
    [InlineData("floor(5.0)", 5.0)]
    public void FloorFunction_ShouldFloor(string expression, double expected)
    {
        // Act
        double result = EvaluateNumeric(expression);

        // Assert
        result.ShouldBe(expected, 0.00001);
    }

    [Theory]
    [InlineData("ceiling(1.1)", 2.0)]
    [InlineData("ceiling(1.9)", 2.0)]
    [InlineData("ceiling(-1.9)", -1.0)]
    [InlineData("ceiling(5.0)", 5.0)]
    public void CeilingFunction_ShouldCeiling(string expression, double expected)
    {
        // Act
        double result = EvaluateNumeric(expression);

        // Assert
        result.ShouldBe(expected, 0.00001);
    }

    #endregion

    #region Aggregate Functions

    [Theory]
    [InlineData("sum((1, 2, 3))", 6.0)]
    [InlineData("sum((-1, 1))", 0.0)]
    public void SumFunction_ShouldSum(string expression, double expected)
    {
        // Act
        double result = EvaluateNumeric(expression);

        // Assert
        result.ShouldBe(expected, 0.00001);
    }

    [Theory]
    [InlineData("avg((2, 4, 6))", 4.0)]
    [InlineData("avg((1, 2, 3, 4, 5))", 3.0)]
    public void AvgFunction_ShouldAverage(string expression, double expected)
    {
        // Act
        double result = EvaluateNumeric(expression);

        // Assert
        result.ShouldBe(expected, 0.00001);
    }

    [Theory]
    [InlineData("min((3, 1, 2))", 1.0)]
    [InlineData("min((-5, 0, 5))", -5.0)]
    public void MinFunction_ShouldReturnMinimum(string expression, double expected)
    {
        // Act
        double result = EvaluateNumeric(expression);

        // Assert
        result.ShouldBe(expected, 0.00001);
    }

    [Theory]
    [InlineData("max((3, 1, 2))", 3.0)]
    [InlineData("max((-5, 0, 5))", 5.0)]
    public void MaxFunction_ShouldReturnMaximum(string expression, double expected)
    {
        // Act
        double result = EvaluateNumeric(expression);

        // Assert
        result.ShouldBe(expected, 0.00001);
    }

    #endregion

    #region Sequence Construction

    [Fact]
    public void NestedSequence_ShouldFlatten()
    {
        // In XPath/Metapath, sequences are always flat
        // Act
        var result = Evaluate("((1, 2), (3, 4))");

        // Assert
        result.Count.ShouldBe(4);
    }

    #region Additional Sequence Functions

    [Fact]
    public void DistinctValuesFunction_ShouldRemoveDuplicates()
    {
        // Act
        var result = Evaluate("distinct-values((1, 2, 2, 3, 3, 3))");

        // Assert
        result.Count.ShouldBe(3);
        ((IntegerItem)result.ToList()[0]).Value.ShouldBe(1);
        ((IntegerItem)result.ToList()[1]).Value.ShouldBe(2);
        ((IntegerItem)result.ToList()[2]).Value.ShouldBe(3);
    }

    [Fact]
    public void IndexOfFunction_ShouldReturnMatchingPositions()
    {
        // Act
        var result = Evaluate("index-of((10, 20, 30, 20, 10), 20)");

        // Assert
        result.Count.ShouldBe(2);
        ((IntegerItem)result.ToList()[0]).Value.ShouldBe(2);
        ((IntegerItem)result.ToList()[1]).Value.ShouldBe(4);
    }

    [Fact]
    public void IndexOfFunction_NoMatch_ShouldReturnEmpty()
    {
        // Act
        var result = Evaluate("index-of((1, 2, 3), 99)");

        // Assert
        result.Count.ShouldBe(0);
    }

    [Fact]
    public void ReverseFunction_ShouldReverseSequence()
    {
        // Act
        var result = Evaluate("reverse((1, 2, 3, 4, 5))");

        // Assert
        result.Count.ShouldBe(5);
        ((IntegerItem)result.ToList()[0]).Value.ShouldBe(5);
        ((IntegerItem)result.ToList()[1]).Value.ShouldBe(4);
        ((IntegerItem)result.ToList()[2]).Value.ShouldBe(3);
        ((IntegerItem)result.ToList()[3]).Value.ShouldBe(2);
        ((IntegerItem)result.ToList()[4]).Value.ShouldBe(1);
    }

    [Fact]
    public void SubsequenceFunction_WithStartOnly_ShouldReturnFromStart()
    {
        // Act
        var result = Evaluate("subsequence((1, 2, 3, 4, 5), 3)");

        // Assert
        result.Count.ShouldBe(3);
        ((IntegerItem)result.ToList()[0]).Value.ShouldBe(3);
        ((IntegerItem)result.ToList()[1]).Value.ShouldBe(4);
        ((IntegerItem)result.ToList()[2]).Value.ShouldBe(5);
    }

    [Fact]
    public void SubsequenceFunction_WithStartAndLength_ShouldReturnSubset()
    {
        // Act
        var result = Evaluate("subsequence((1, 2, 3, 4, 5), 2, 3)");

        // Assert
        result.Count.ShouldBe(3);
        ((IntegerItem)result.ToList()[0]).Value.ShouldBe(2);
        ((IntegerItem)result.ToList()[1]).Value.ShouldBe(3);
        ((IntegerItem)result.ToList()[2]).Value.ShouldBe(4);
    }

    [Fact]
    public void InsertBeforeFunction_ShouldInsertAtPosition()
    {
        // Act
        var result = Evaluate("insert-before((1, 2, 3), 2, (10, 20))");

        // Assert
        result.Count.ShouldBe(5);
        ((IntegerItem)result.ToList()[0]).Value.ShouldBe(1);
        ((IntegerItem)result.ToList()[1]).Value.ShouldBe(10);
        ((IntegerItem)result.ToList()[2]).Value.ShouldBe(20);
        ((IntegerItem)result.ToList()[3]).Value.ShouldBe(2);
        ((IntegerItem)result.ToList()[4]).Value.ShouldBe(3);
    }

    [Fact]
    public void RemoveFunction_ShouldRemoveAtPosition()
    {
        // Act
        var result = Evaluate("remove((1, 2, 3, 4, 5), 3)");

        // Assert
        result.Count.ShouldBe(4);
        ((IntegerItem)result.ToList()[0]).Value.ShouldBe(1);
        ((IntegerItem)result.ToList()[1]).Value.ShouldBe(2);
        ((IntegerItem)result.ToList()[2]).Value.ShouldBe(4);
        ((IntegerItem)result.ToList()[3]).Value.ShouldBe(5);
    }

    [Theory]
    [InlineData("deep-equal((1, 2, 3), (1, 2, 3))", true)]
    [InlineData("deep-equal((1, 2), (1, 2, 3))", false)]
    [InlineData("deep-equal((), ())", true)]
    public void DeepEqualFunction_ShouldCompareSequences(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void ZeroOrOneFunction_WithEmpty_ShouldReturnEmpty()
    {
        // Act
        var result = Evaluate("zero-or-one(())");

        // Assert
        result.Count.ShouldBe(0);
    }

    [Fact]
    public void ZeroOrOneFunction_WithOne_ShouldReturnItem()
    {
        // Act
        long result = EvaluateInteger("zero-or-one((42))");

        // Assert
        result.ShouldBe(42);
    }

    [Fact]
    public void ZeroOrOneFunction_WithMultiple_ShouldThrow()
    {
        // Act & Assert
        Should.Throw<MetapathException>(() => Evaluate("zero-or-one((1, 2, 3))"));
    }

    [Fact]
    public void OneOrMoreFunction_WithEmpty_ShouldThrow()
    {
        // Act & Assert
        Should.Throw<MetapathException>(() => Evaluate("one-or-more(())"));
    }

    [Fact]
    public void OneOrMoreFunction_WithItems_ShouldReturnSequence()
    {
        // Act
        var result = Evaluate("one-or-more((1, 2, 3))");

        // Assert
        result.Count.ShouldBe(3);
    }

    [Fact]
    public void ExactlyOneFunction_WithOne_ShouldReturnItem()
    {
        // Act
        long result = EvaluateInteger("exactly-one((42))");

        // Assert
        result.ShouldBe(42);
    }

    [Fact]
    public void ExactlyOneFunction_WithEmpty_ShouldThrow()
    {
        // Act & Assert
        Should.Throw<MetapathException>(() => Evaluate("exactly-one(())"));
    }

    [Fact]
    public void ExactlyOneFunction_WithMultiple_ShouldThrow()
    {
        // Act & Assert
        Should.Throw<MetapathException>(() => Evaluate("exactly-one((1, 2))"));
    }

    #endregion

    #region Additional String Functions

    [Theory]
    [InlineData("string-join(('a', 'b', 'c'), '-')", "a-b-c")]
    [InlineData("string-join(('hello', 'world'), ' ')", "hello world")]
    [InlineData("string-join(('a', 'b', 'c'), '')", "abc")]
    [InlineData("string-join(('single'), ',')", "single")]
    [InlineData("string-join((), ',')", "")]
    public void StringJoinFunction_ShouldJoinWithSeparator(string expression, string expected)
    {
        // Act
        string? result = EvaluateString(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void StringJoinFunction_WithoutSeparator_ShouldConcatenate()
    {
        // Act
        string? result = EvaluateString("string-join(('a', 'b', 'c'))");

        // Assert
        result.ShouldBe("abc");
    }

    [Theory]
    [InlineData("substring-before('hello-world', '-')", "hello")]
    [InlineData("substring-before('abc', 'b')", "a")]
    [InlineData("substring-before('hello', 'x')", "")]
    [InlineData("substring-before('hello', '')", "")]
    public void SubstringBeforeFunction_ShouldReturnPrefix(string expression, string expected)
    {
        // Act
        string? result = EvaluateString(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("substring-after('hello-world', '-')", "world")]
    [InlineData("substring-after('abc', 'a')", "bc")]
    [InlineData("substring-after('hello', 'x')", "")]
    [InlineData("substring-after('hello', '')", "hello")]
    public void SubstringAfterFunction_ShouldReturnSuffix(string expression, string expected)
    {
        // Act
        string? result = EvaluateString(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("translate('abc', 'abc', 'ABC')", "ABC")]
    [InlineData("translate('hello', 'el', 'ip')", "hippo")]
    [InlineData("translate('--aaa--', 'abc-', 'ABC')", "AAA")]
    [InlineData("translate('abc', 'b', '')", "ac")]
    public void TranslateFunction_ShouldReplaceCharacters(string expression, string expected)
    {
        // Act
        string? result = EvaluateString(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("compare('a', 'b')", -1)]
    [InlineData("compare('b', 'a')", 1)]
    [InlineData("compare('hello', 'hello')", 0)]
    [InlineData("compare('abc', 'abd')", -1)]
    public void CompareFunction_ShouldCompareStrings(string expression, long expected)
    {
        // Act
        long result = EvaluateInteger(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void CodepointsToStringFunction_ShouldConvert()
    {
        // Act
        string? result = EvaluateString("codepoints-to-string((72, 105))");

        // Assert
        result.ShouldBe("Hi");
    }

    [Fact]
    public void StringToCodepointsFunction_ShouldConvert()
    {
        // Act
        var result = Evaluate("string-to-codepoints('Hi')");

        // Assert
        result.Count.ShouldBe(2);
        ((IntegerItem)result.ToList()[0]).Value.ShouldBe(72); // 'H'
        ((IntegerItem)result.ToList()[1]).Value.ShouldBe(105); // 'i'
    }

    #endregion

    #region Regex Functions

    [Theory]
    [InlineData("matches('hello', 'ell')", true)]
    [InlineData("matches('hello', '^h.*o$')", true)]
    [InlineData("matches('hello', 'xyz')", false)]
    [InlineData("matches('Hello', 'hello')", false)]
    [InlineData("matches('Hello', 'hello', 'i')", true)]
    public void MatchesFunction_ShouldMatchPattern(string expression, bool expected)
    {
        // Act
        bool result = EvaluateBoolean(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("replace('hello', 'l', 'L')", "heLLo")]
    [InlineData("replace('aaa', 'a', 'b')", "bbb")]
    [InlineData("replace('hello world', 'world', 'universe')", "hello universe")]
    [InlineData("replace('abc123', '[0-9]+', 'X')", "abcX")]
    public void ReplaceFunction_ShouldReplacePattern(string expression, string expected)
    {
        // Act
        string? result = EvaluateString(expression);

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void TokenizeFunction_WithPattern_ShouldSplit()
    {
        // Act
        var result = Evaluate("tokenize('a,b,c', ',')");

        // Assert
        result.Count.ShouldBe(3);
        ((StringItem)result.ToList()[0]).Value.ShouldBe("a");
        ((StringItem)result.ToList()[1]).Value.ShouldBe("b");
        ((StringItem)result.ToList()[2]).Value.ShouldBe("c");
    }

    [Fact]
    public void TokenizeFunction_WithWhitespace_ShouldSplitOnWhitespace()
    {
        // Default pattern is \s+
        // Act
        var result = Evaluate("tokenize('hello world  test')");

        // Assert
        result.Count.ShouldBe(3);
        ((StringItem)result.ToList()[0]).Value.ShouldBe("hello");
        ((StringItem)result.ToList()[1]).Value.ShouldBe("world");
        ((StringItem)result.ToList()[2]).Value.ShouldBe("test");
    }

    [Fact]
    public void TokenizeFunction_WithRegex_ShouldSplit()
    {
        // Act
        var result = Evaluate("tokenize('abc123def456', '[0-9]+')");

        // Assert
        result.Count.ShouldBe(2);
        ((StringItem)result.ToList()[0]).Value.ShouldBe("abc");
        ((StringItem)result.ToList()[1]).Value.ShouldBe("def");
    }

    #endregion

    #region Numeric Conversion Functions

    [Theory]
    [InlineData("number('123')", 123.0)]
    [InlineData("number('3.14')", 3.14)]
    [InlineData("number('-42')", -42.0)]
    public void NumberFunction_ShouldConvert(string expression, double expected)
    {
        // Act
        double result = EvaluateNumeric(expression);

        // Assert
        result.ShouldBe(expected, 0.00001);
    }

    [Fact]
    public void NumberFunction_WithNonNumeric_ShouldReturnNaN()
    {
        // Act
        double result = EvaluateNumeric("number('hello')");

        // Assert
        double.IsNaN(result).ShouldBeTrue();
    }

    #endregion

    #region Sequence Operators (Intersect/Except/Union)

    [Fact]
    public void IntersectOperator_ShouldReturnCommonItems()
    {
        // Act
        var result = Evaluate("(1, 2, 3, 4, 5) intersect (3, 4, 5, 6, 7)");

        // Assert - intersection should return items in both sequences (by value comparison)
        result.Count.ShouldBe(3);
        var values = result.Select(i => ((IntegerItem)i).Value).OrderBy(v => v).ToList();
        values.ShouldBe([3, 4, 5]);
    }

    [Fact]
    public void IntersectOperator_NoCommon_ShouldReturnEmpty()
    {
        // Act
        var result = Evaluate("(1, 2, 3) intersect (4, 5, 6)");

        // Assert
        result.Count.ShouldBe(0);
    }

    [Fact]
    public void ExceptOperator_ShouldReturnDifference()
    {
        // Act
        var result = Evaluate("(1, 2, 3, 4, 5) except (3, 4)");

        // Assert - except should return items from first but not in second
        result.Count.ShouldBe(3);
        var values = result.Select(i => ((IntegerItem)i).Value).OrderBy(v => v).ToList();
        values.ShouldBe([1, 2, 5]);
    }

    [Fact]
    public void ExceptOperator_AllRemoved_ShouldReturnEmpty()
    {
        // Act
        var result = Evaluate("(1, 2, 3) except (1, 2, 3, 4, 5)");

        // Assert
        result.Count.ShouldBe(0);
    }

    #endregion

    #endregion

    #region Function Composition

    [Fact]
    public void NestedFunctionCalls_ShouldEvaluate()
    {
        // Act
        string? result = EvaluateString("upper-case(concat('hello', ' ', 'world'))");

        // Assert
        result.ShouldBe("HELLO WORLD");
    }

    [Fact]
    public void FunctionWithArithmeticArgument_ShouldEvaluate()
    {
        // Act
        double result = EvaluateNumeric("abs(-5 - 5)");

        // Assert
        result.ShouldBe(10.0, 0.00001);
    }

    [Fact]
    public void FunctionWithComparisonArgument_ShouldEvaluate()
    {
        // Act
        bool result = EvaluateBoolean("not(1 + 1 eq 3)");

        // Assert
        result.ShouldBeTrue();
    }

    #endregion

    #region Date/Time Functions

    [Fact]
    public void CurrentDateTimeFunction_ShouldReturnDateTimeItem()
    {
        // Act
        var result = EvaluateSingle("current-dateTime()");

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<DateTimeItem>();
    }

    [Fact]
    public void CurrentDateFunction_ShouldReturnDateItem()
    {
        // Act
        var result = EvaluateSingle("current-date()");

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<DateItem>();
    }

    [Fact]
    public void CurrentTimeFunction_ShouldReturnTimeItem()
    {
        // Act
        var result = EvaluateSingle("current-time()");

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<TimeItem>();
    }

    [Fact]
    public void ImplicitTimezoneFunction_ShouldReturnDuration()
    {
        // Act
        var result = EvaluateSingle("implicit-timezone()");

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<DayTimeDurationItem>();
    }

    [Fact]
    public void YearFromDateTimeFunction_ShouldExtractYear()
    {
        // Arrange - using DateTimeItem directly since we can't parse dateTime literals yet
        var dt = new DateTimeItem(new DateTimeOffset(2024, 6, 15, 10, 30, 0, TimeSpan.Zero));
        var dynamicContext = new DynamicContext().WithContextItem(dt);
        var context = new MetapathContext(StaticContext.CreateDefault(), dynamicContext);

        // Act
        var expr = MetapathExpression.Compile("year-from-dateTime(.)");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<IntegerItem>();
        ((IntegerItem)result).Value.ShouldBe(2024);
    }

    [Fact]
    public void MonthFromDateTimeFunction_ShouldExtractMonth()
    {
        // Arrange
        var dt = new DateTimeItem(new DateTimeOffset(2024, 6, 15, 10, 30, 0, TimeSpan.Zero));
        var dynamicContext = new DynamicContext().WithContextItem(dt);
        var context = new MetapathContext(StaticContext.CreateDefault(), dynamicContext);

        // Act
        var expr = MetapathExpression.Compile("month-from-dateTime(.)");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<IntegerItem>();
        ((IntegerItem)result).Value.ShouldBe(6);
    }

    [Fact]
    public void DayFromDateTimeFunction_ShouldExtractDay()
    {
        // Arrange
        var dt = new DateTimeItem(new DateTimeOffset(2024, 6, 15, 10, 30, 0, TimeSpan.Zero));
        var dynamicContext = new DynamicContext().WithContextItem(dt);
        var context = new MetapathContext(StaticContext.CreateDefault(), dynamicContext);

        // Act
        var expr = MetapathExpression.Compile("day-from-dateTime(.)");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<IntegerItem>();
        ((IntegerItem)result).Value.ShouldBe(15);
    }

    [Fact]
    public void HoursFromDateTimeFunction_ShouldExtractHours()
    {
        // Arrange
        var dt = new DateTimeItem(new DateTimeOffset(2024, 6, 15, 10, 30, 45, TimeSpan.Zero));
        var dynamicContext = new DynamicContext().WithContextItem(dt);
        var context = new MetapathContext(StaticContext.CreateDefault(), dynamicContext);

        // Act
        var expr = MetapathExpression.Compile("hours-from-dateTime(.)");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<IntegerItem>();
        ((IntegerItem)result).Value.ShouldBe(10);
    }

    [Fact]
    public void MinutesFromDateTimeFunction_ShouldExtractMinutes()
    {
        // Arrange
        var dt = new DateTimeItem(new DateTimeOffset(2024, 6, 15, 10, 30, 45, TimeSpan.Zero));
        var dynamicContext = new DynamicContext().WithContextItem(dt);
        var context = new MetapathContext(StaticContext.CreateDefault(), dynamicContext);

        // Act
        var expr = MetapathExpression.Compile("minutes-from-dateTime(.)");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<IntegerItem>();
        ((IntegerItem)result).Value.ShouldBe(30);
    }

    [Fact]
    public void SecondsFromDateTimeFunction_ShouldExtractSeconds()
    {
        // Arrange
        var dt = new DateTimeItem(new DateTimeOffset(2024, 6, 15, 10, 30, 45, TimeSpan.Zero));
        var dynamicContext = new DynamicContext().WithContextItem(dt);
        var context = new MetapathContext(StaticContext.CreateDefault(), dynamicContext);

        // Act
        var expr = MetapathExpression.Compile("seconds-from-dateTime(.)");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<DecimalItem>();
        ((DecimalItem)result).Value.ShouldBe(45);
    }

    [Fact]
    public void YearFromDateFunction_ShouldExtractYear()
    {
        // Arrange
        var date = new DateItem(new DateOnly(2024, 6, 15));
        var dynamicContext = new DynamicContext().WithContextItem(date);
        var context = new MetapathContext(StaticContext.CreateDefault(), dynamicContext);

        // Act
        var expr = MetapathExpression.Compile("year-from-date(.)");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<IntegerItem>();
        ((IntegerItem)result).Value.ShouldBe(2024);
    }

    [Fact]
    public void MonthFromDateFunction_ShouldExtractMonth()
    {
        // Arrange
        var date = new DateItem(new DateOnly(2024, 6, 15));
        var dynamicContext = new DynamicContext().WithContextItem(date);
        var context = new MetapathContext(StaticContext.CreateDefault(), dynamicContext);

        // Act
        var expr = MetapathExpression.Compile("month-from-date(.)");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<IntegerItem>();
        ((IntegerItem)result).Value.ShouldBe(6);
    }

    [Fact]
    public void DayFromDateFunction_ShouldExtractDay()
    {
        // Arrange
        var date = new DateItem(new DateOnly(2024, 6, 15));
        var dynamicContext = new DynamicContext().WithContextItem(date);
        var context = new MetapathContext(StaticContext.CreateDefault(), dynamicContext);

        // Act
        var expr = MetapathExpression.Compile("day-from-date(.)");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<IntegerItem>();
        ((IntegerItem)result).Value.ShouldBe(15);
    }

    [Fact]
    public void HoursFromTimeFunction_ShouldExtractHours()
    {
        // Arrange
        var time = new TimeItem(new TimeOnly(14, 30, 45));
        var dynamicContext = new DynamicContext().WithContextItem(time);
        var context = new MetapathContext(StaticContext.CreateDefault(), dynamicContext);

        // Act
        var expr = MetapathExpression.Compile("hours-from-time(.)");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<IntegerItem>();
        ((IntegerItem)result).Value.ShouldBe(14);
    }

    [Fact]
    public void MinutesFromTimeFunction_ShouldExtractMinutes()
    {
        // Arrange
        var time = new TimeItem(new TimeOnly(14, 30, 45));
        var dynamicContext = new DynamicContext().WithContextItem(time);
        var context = new MetapathContext(StaticContext.CreateDefault(), dynamicContext);

        // Act
        var expr = MetapathExpression.Compile("minutes-from-time(.)");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<IntegerItem>();
        ((IntegerItem)result).Value.ShouldBe(30);
    }

    [Fact]
    public void SecondsFromTimeFunction_ShouldExtractSeconds()
    {
        // Arrange
        var time = new TimeItem(new TimeOnly(14, 30, 45));
        var dynamicContext = new DynamicContext().WithContextItem(time);
        var context = new MetapathContext(StaticContext.CreateDefault(), dynamicContext);

        // Act
        var expr = MetapathExpression.Compile("seconds-from-time(.)");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<DecimalItem>();
        ((DecimalItem)result).Value.ShouldBe(45);
    }

    [Fact]
    public void YearsFromDurationFunction_ShouldExtractYears()
    {
        // Arrange - 2 years 3 months
        var duration = DurationItem.YearMonth(2, 3);
        var dynamicContext = new DynamicContext().WithContextItem(duration);
        var context = new MetapathContext(StaticContext.CreateDefault(), dynamicContext);

        // Act
        var expr = MetapathExpression.Compile("years-from-duration(.)");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<IntegerItem>();
        ((IntegerItem)result).Value.ShouldBe(2);
    }

    [Fact]
    public void MonthsFromDurationFunction_ShouldExtractMonths()
    {
        // Arrange - 2 years 3 months
        var duration = DurationItem.YearMonth(2, 3);
        var dynamicContext = new DynamicContext().WithContextItem(duration);
        var context = new MetapathContext(StaticContext.CreateDefault(), dynamicContext);

        // Act
        var expr = MetapathExpression.Compile("months-from-duration(.)");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<IntegerItem>();
        ((IntegerItem)result).Value.ShouldBe(3);
    }

    [Fact]
    public void DaysFromDurationFunction_ShouldExtractDays()
    {
        // Arrange - 5 days 3 hours
        var duration = DurationItem.DayTime(TimeSpan.FromDays(5) + TimeSpan.FromHours(3));
        var dynamicContext = new DynamicContext().WithContextItem(duration);
        var context = new MetapathContext(StaticContext.CreateDefault(), dynamicContext);

        // Act
        var expr = MetapathExpression.Compile("days-from-duration(.)");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<IntegerItem>();
        ((IntegerItem)result).Value.ShouldBe(5);
    }

    [Fact]
    public void HoursFromDurationFunction_ShouldExtractHours()
    {
        // Arrange - 5 days 3 hours
        var duration = DurationItem.DayTime(TimeSpan.FromDays(5) + TimeSpan.FromHours(3));
        var dynamicContext = new DynamicContext().WithContextItem(duration);
        var context = new MetapathContext(StaticContext.CreateDefault(), dynamicContext);

        // Act
        var expr = MetapathExpression.Compile("hours-from-duration(.)");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<IntegerItem>();
        ((IntegerItem)result).Value.ShouldBe(3);
    }

    #endregion

    #region Array Functions

    [Fact]
    public void ArraySizeFunction_ShouldReturnCount()
    {
        // Arrange - create an array with 3 members
        var array = ArrayItem.Of([
            Sequence.Of(IntegerItem.Of(1)),
            Sequence.Of(IntegerItem.Of(2)),
            Sequence.Of(IntegerItem.Of(3))
        ]);
        var dynamicContext = new DynamicContext().WithContextItem(array);
        var context = new MetapathContext(StaticContext.CreateDefault(), dynamicContext);

        // Act - using array namespace prefix
        var staticContext = StaticContext.CreateDefault();
        staticContext.RegisterNamespace("array", Functions.FunctionLibrary.ArrayNamespace);
        var ctx = new MetapathContext(staticContext, dynamicContext);
        var expr = MetapathExpression.Compile("array:size(.)");
        var result = expr.EvaluateSingle(ctx);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<IntegerItem>();
        ((IntegerItem)result).Value.ShouldBe(3);
    }

    [Fact]
    public void ArrayGetFunction_ShouldReturnMemberAtIndex()
    {
        // Arrange
        var array = ArrayItem.Of([
            Sequence.Of(StringItem.Of("first")),
            Sequence.Of(StringItem.Of("second")),
            Sequence.Of(StringItem.Of("third"))
        ]);
        var dynamicContext = new DynamicContext().WithContextItem(array);
        var staticContext = StaticContext.CreateDefault();
        staticContext.RegisterNamespace("array", Functions.FunctionLibrary.ArrayNamespace);
        var context = new MetapathContext(staticContext, dynamicContext);

        // Act - get second element (1-based indexing)
        var expr = MetapathExpression.Compile("array:get(., 2)");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<StringItem>();
        ((StringItem)result).Value.ShouldBe("second");
    }

    [Fact]
    public void ArrayHeadFunction_ShouldReturnFirstMember()
    {
        // Arrange
        var array = ArrayItem.Of([
            Sequence.Of(IntegerItem.Of(10)),
            Sequence.Of(IntegerItem.Of(20)),
            Sequence.Of(IntegerItem.Of(30))
        ]);
        var dynamicContext = new DynamicContext().WithContextItem(array);
        var staticContext = StaticContext.CreateDefault();
        staticContext.RegisterNamespace("array", Functions.FunctionLibrary.ArrayNamespace);
        var context = new MetapathContext(staticContext, dynamicContext);

        // Act
        var expr = MetapathExpression.Compile("array:head(.)");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<IntegerItem>();
        ((IntegerItem)result).Value.ShouldBe(10);
    }

    [Fact]
    public void ArrayTailFunction_ShouldReturnRemainingArray()
    {
        // Arrange
        var array = ArrayItem.Of([
            Sequence.Of(IntegerItem.Of(10)),
            Sequence.Of(IntegerItem.Of(20)),
            Sequence.Of(IntegerItem.Of(30))
        ]);
        var dynamicContext = new DynamicContext().WithContextItem(array);
        var staticContext = StaticContext.CreateDefault();
        staticContext.RegisterNamespace("array", Functions.FunctionLibrary.ArrayNamespace);
        var context = new MetapathContext(staticContext, dynamicContext);

        // Act
        var expr = MetapathExpression.Compile("array:tail(.)");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<ArrayItem>();
        var tailArray = (ArrayItem)result;
        tailArray.Size.ShouldBe(2);
    }

    [Fact]
    public void ArrayReverseFunction_ShouldReverseOrder()
    {
        // Arrange
        var array = ArrayItem.Of([
            Sequence.Of(IntegerItem.Of(1)),
            Sequence.Of(IntegerItem.Of(2)),
            Sequence.Of(IntegerItem.Of(3))
        ]);
        var dynamicContext = new DynamicContext().WithContextItem(array);
        var staticContext = StaticContext.CreateDefault();
        staticContext.RegisterNamespace("array", Functions.FunctionLibrary.ArrayNamespace);
        var context = new MetapathContext(staticContext, dynamicContext);

        // Act
        var expr = MetapathExpression.Compile("array:reverse(.)");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<ArrayItem>();
        var reversedArray = (ArrayItem)result;
        reversedArray.Size.ShouldBe(3);
        ((IntegerItem)reversedArray.Get(1).FirstOrDefault!).Value.ShouldBe(3);
        ((IntegerItem)reversedArray.Get(2).FirstOrDefault!).Value.ShouldBe(2);
        ((IntegerItem)reversedArray.Get(3).FirstOrDefault!).Value.ShouldBe(1);
    }

    #endregion

    #region Map Functions

    [Fact]
    public void MapSizeFunction_ShouldReturnEntryCount()
    {
        // Arrange
        var entries = new Dictionary<IAtomicItem, ISequence>
        {
            { StringItem.Of("a"), Sequence.Of(IntegerItem.Of(1)) },
            { StringItem.Of("b"), Sequence.Of(IntegerItem.Of(2)) },
            { StringItem.Of("c"), Sequence.Of(IntegerItem.Of(3)) }
        };
        var map = MapItem.Of(entries);
        var dynamicContext = new DynamicContext().WithContextItem(map);
        var staticContext = StaticContext.CreateDefault();
        staticContext.RegisterNamespace("map", Functions.FunctionLibrary.MapNamespace);
        var context = new MetapathContext(staticContext, dynamicContext);

        // Act
        var expr = MetapathExpression.Compile("map:size(.)");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<IntegerItem>();
        ((IntegerItem)result).Value.ShouldBe(3);
    }

    [Fact]
    public void MapContainsFunction_ShouldCheckForKey()
    {
        // Arrange
        var entries = new Dictionary<IAtomicItem, ISequence>
        {
            { StringItem.Of("name"), Sequence.Of(StringItem.Of("John")) },
            { StringItem.Of("age"), Sequence.Of(IntegerItem.Of(30)) }
        };
        var map = MapItem.Of(entries);
        var dynamicContext = new DynamicContext().WithContextItem(map);
        var staticContext = StaticContext.CreateDefault();
        staticContext.RegisterNamespace("map", Functions.FunctionLibrary.MapNamespace);
        var context = new MetapathContext(staticContext, dynamicContext);

        // Act - check for existing key
        var expr = MetapathExpression.Compile("map:contains(., 'name')");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<BooleanItem>();
        ((BooleanItem)result).Value.ShouldBeTrue();

        // Act - check for non-existing key
        var expr2 = MetapathExpression.Compile("map:contains(., 'missing')");
        var result2 = expr2.EvaluateSingle(context);
        ((BooleanItem)result2!).Value.ShouldBeFalse();
    }

    [Fact]
    public void MapGetFunction_ShouldReturnValueForKey()
    {
        // Arrange
        var entries = new Dictionary<IAtomicItem, ISequence>
        {
            { StringItem.Of("name"), Sequence.Of(StringItem.Of("John")) },
            { StringItem.Of("age"), Sequence.Of(IntegerItem.Of(30)) }
        };
        var map = MapItem.Of(entries);
        var dynamicContext = new DynamicContext().WithContextItem(map);
        var staticContext = StaticContext.CreateDefault();
        staticContext.RegisterNamespace("map", Functions.FunctionLibrary.MapNamespace);
        var context = new MetapathContext(staticContext, dynamicContext);

        // Act
        var expr = MetapathExpression.Compile("map:get(., 'name')");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<StringItem>();
        ((StringItem)result).Value.ShouldBe("John");
    }

    [Fact]
    public void MapKeysFunction_ShouldReturnAllKeys()
    {
        // Arrange
        var entries = new Dictionary<IAtomicItem, ISequence>
        {
            { StringItem.Of("a"), Sequence.Of(IntegerItem.Of(1)) },
            { StringItem.Of("b"), Sequence.Of(IntegerItem.Of(2)) }
        };
        var map = MapItem.Of(entries);
        var dynamicContext = new DynamicContext().WithContextItem(map);
        var staticContext = StaticContext.CreateDefault();
        staticContext.RegisterNamespace("map", Functions.FunctionLibrary.MapNamespace);
        var context = new MetapathContext(staticContext, dynamicContext);

        // Act
        var expr = MetapathExpression.Compile("map:keys(.)");
        var result = expr.Evaluate(context);

        // Assert
        result.Count.ShouldBe(2);
        var keys = result.Select(i => ((StringItem)i).Value).ToHashSet();
        keys.ShouldContain("a");
        keys.ShouldContain("b");
    }

    #endregion

    #region QName Functions

    [Fact]
    public void QNameFunction_ShouldConstructQName()
    {
        // Act
        var result = EvaluateSingle("QName('http://example.com', 'ex:element')");

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<QNameItem>();
        var qn = (QNameItem)result;
        qn.LocalName.ShouldBe("element");
        qn.Prefix.ShouldBe("ex");
        qn.NamespaceUri.ShouldBe("http://example.com");
    }

    [Fact]
    public void LocalNameFromQNameFunction_ShouldExtractLocalName()
    {
        // Arrange
        var qname = QNameItem.Of("ex", "http://example.com", "element");
        var dynamicContext = new DynamicContext().WithContextItem(qname);
        var context = new MetapathContext(StaticContext.CreateDefault(), dynamicContext);

        // Act
        var expr = MetapathExpression.Compile("local-name-from-QName(.)");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<StringItem>();
        ((StringItem)result).Value.ShouldBe("element");
    }

    [Fact]
    public void NamespaceUriFromQNameFunction_ShouldExtractNamespace()
    {
        // Arrange
        var qname = QNameItem.Of("ex", "http://example.com", "element");
        var dynamicContext = new DynamicContext().WithContextItem(qname);
        var context = new MetapathContext(StaticContext.CreateDefault(), dynamicContext);

        // Act
        var expr = MetapathExpression.Compile("namespace-uri-from-QName(.)");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<StringItem>();
        ((StringItem)result).Value.ShouldBe("http://example.com");
    }

    [Fact]
    public void PrefixFromQNameFunction_ShouldExtractPrefix()
    {
        // Arrange
        var qname = QNameItem.Of("ex", "http://example.com", "element");
        var dynamicContext = new DynamicContext().WithContextItem(qname);
        var context = new MetapathContext(StaticContext.CreateDefault(), dynamicContext);

        // Act
        var expr = MetapathExpression.Compile("prefix-from-QName(.)");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<StringItem>();
        ((StringItem)result).Value.ShouldBe("ex");
    }

    #endregion

    #region URI Functions

    [Fact]
    public void EncodeForUriFunction_ShouldEscapeSpecialCharacters()
    {
        // Act
        string? result = EvaluateString("encode-for-uri('hello world')");

        // Assert
        result.ShouldBe("hello%20world");
    }

    [Theory]
    [InlineData("encode-for-uri('abc')", "abc")]
    [InlineData("encode-for-uri('a b')", "a%20b")]
    [InlineData("encode-for-uri('a&b')", "a%26b")]
    public void EncodeForUriFunction_ShouldEncodeCorrectly(string expression, string expected)
    {
        // Act
        string? result = EvaluateString(expression);

        // Assert
        result.ShouldBe(expected);
    }

    #endregion

    #region Metaschema-Specific Functions

    [Fact]
    public void Base64EncodeFunction_ShouldEncodeString()
    {
        // Arrange
        var staticContext = StaticContext.CreateDefault();
        staticContext.RegisterNamespace("mp", Functions.FunctionLibrary.MetaschemaNamespace);
        var context = new MetapathContext(staticContext, new DynamicContext());

        // Act
        var expr = MetapathExpression.Compile("mp:base64-encode('Hello')");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<StringItem>();
        ((StringItem)result).Value.ShouldBe("SGVsbG8=");
    }

    [Fact]
    public void Base64DecodeFunction_ShouldDecodeString()
    {
        // Arrange
        var staticContext = StaticContext.CreateDefault();
        staticContext.RegisterNamespace("mp", Functions.FunctionLibrary.MetaschemaNamespace);
        var context = new MetapathContext(staticContext, new DynamicContext());

        // Act
        var expr = MetapathExpression.Compile("mp:base64-decode('SGVsbG8=')");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<StringItem>();
        ((StringItem)result).Value.ShouldBe("Hello");
    }

    [Fact]
    public void Base64RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var staticContext = StaticContext.CreateDefault();
        staticContext.RegisterNamespace("mp", Functions.FunctionLibrary.MetaschemaNamespace);
        var context = new MetapathContext(staticContext, new DynamicContext());

        // Act - encode then decode
        var expr = MetapathExpression.Compile("mp:base64-decode(mp:base64-encode('Test String 123!'))");
        var result = expr.EvaluateSingle(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<StringItem>();
        ((StringItem)result).Value.ShouldBe("Test String 123!");
    }

    #endregion
}
