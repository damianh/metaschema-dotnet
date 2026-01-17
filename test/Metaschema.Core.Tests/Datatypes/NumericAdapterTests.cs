// Licensed under the MIT License.

using Metaschema.Core.Datatypes;
using Metaschema.Core.Datatypes.Adapters;
using Shouldly;
using Xunit;

namespace Metaschema.Core.Tests.Datatypes;

public class IntegerAdapterTests
{
    [Theory]
    [InlineData("0", 0L)]
    [InlineData("42", 42L)]
    [InlineData("-42", -42L)]
    [InlineData("9223372036854775807", long.MaxValue)]
    [InlineData("-9223372036854775808", long.MinValue)]
    [InlineData("  123  ", 123L)]
    public void IntegerAdapter_Parse_ValidValues_ShouldSucceed(string input, long expected)
    {
        var adapter = new IntegerAdapter();
        var result = adapter.Parse(input);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("1.5")]
    [InlineData("9223372036854775808")] // Overflow
    public void IntegerAdapter_Parse_InvalidValues_ShouldThrow(string value)
    {
        var adapter = new IntegerAdapter();
        Should.Throw<DataTypeParseException>(() => adapter.Parse(value));
    }

    [Theory]
    [InlineData(0L, "0")]
    [InlineData(42L, "42")]
    [InlineData(-42L, "-42")]
    public void IntegerAdapter_Format_ShouldReturnCorrectString(long value, string expected)
    {
        var adapter = new IntegerAdapter();
        var result = adapter.Format(value);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("123", true, 123L)]
    [InlineData("abc", false, 0L)]
    public void IntegerAdapter_TryParse_ShouldReturnCorrectResult(string input, bool shouldSucceed, long expectedValue)
    {
        var adapter = new IntegerAdapter();
        var success = adapter.TryParse(input, out var result);
        success.ShouldBe(shouldSucceed);
        if (shouldSucceed)
        {
            result.ShouldBe(expectedValue);
        }
    }
}

public class NonNegativeIntegerAdapterTests
{
    [Theory]
    [InlineData("0", 0UL)]
    [InlineData("42", 42UL)]
    [InlineData("18446744073709551615", ulong.MaxValue)]
    public void NonNegativeIntegerAdapter_Parse_ValidValues_ShouldSucceed(string input, ulong expected)
    {
        var adapter = new NonNegativeIntegerAdapter();
        var result = adapter.Parse(input);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("-42")]
    [InlineData("abc")]
    public void NonNegativeIntegerAdapter_Parse_InvalidValues_ShouldThrow(string value)
    {
        var adapter = new NonNegativeIntegerAdapter();
        Should.Throw<DataTypeParseException>(() => adapter.Parse(value));
    }
}

public class PositiveIntegerAdapterTests
{
    [Theory]
    [InlineData("1", 1UL)]
    [InlineData("42", 42UL)]
    [InlineData("18446744073709551615", ulong.MaxValue)]
    public void PositiveIntegerAdapter_Parse_ValidValues_ShouldSucceed(string input, ulong expected)
    {
        var adapter = new PositiveIntegerAdapter();
        var result = adapter.Parse(input);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("abc")]
    public void PositiveIntegerAdapter_Parse_InvalidValues_ShouldThrow(string value)
    {
        var adapter = new PositiveIntegerAdapter();
        Should.Throw<DataTypeParseException>(() => adapter.Parse(value));
    }

    [Theory]
    [InlineData("0", false, 0UL)]
    [InlineData("1", true, 1UL)]
    [InlineData("abc", false, 0UL)]
    public void PositiveIntegerAdapter_TryParse_ShouldReturnCorrectResult(string input, bool shouldSucceed, ulong expectedValue)
    {
        var adapter = new PositiveIntegerAdapter();
        var success = adapter.TryParse(input, out var result);
        success.ShouldBe(shouldSucceed);
        if (shouldSucceed)
        {
            result.ShouldBe(expectedValue);
        }
    }
}

public class DecimalAdapterTests
{
    [Theory]
    [InlineData("0", 0)]
    [InlineData("1.5", 1.5)]
    [InlineData("-1.5", -1.5)]
    [InlineData("123.456", 123.456)]
    [InlineData("  42.0  ", 42.0)]
    public void DecimalAdapter_Parse_ValidValues_ShouldSucceed(string input, double expected)
    {
        var adapter = new DecimalAdapter();
        var result = adapter.Parse(input);
        result.ShouldBe((decimal)expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("1,5")] // Wrong decimal separator
    public void DecimalAdapter_Parse_InvalidValues_ShouldThrow(string value)
    {
        var adapter = new DecimalAdapter();
        Should.Throw<DataTypeParseException>(() => adapter.Parse(value));
    }

    [Fact]
    public void DecimalAdapter_Format_ShouldUseInvariantCulture()
    {
        var adapter = new DecimalAdapter();
        var result = adapter.Format(1.5m);
        result.ShouldBe("1.5");
    }
}

public class BooleanAdapterTests
{
    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("1", true)]
    [InlineData("0", false)]
    [InlineData("  true  ", true)]
    public void BooleanAdapter_Parse_ValidValues_ShouldSucceed(string input, bool expected)
    {
        var adapter = new BooleanAdapter();
        var result = adapter.Parse(input);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("yes")]
    [InlineData("no")]
    public void BooleanAdapter_Parse_InvalidValues_ShouldThrow(string value)
    {
        var adapter = new BooleanAdapter();
        Should.Throw<DataTypeParseException>(() => adapter.Parse(value));
    }

    [Theory]
    [InlineData("TRUE", true)]
    [InlineData("FALSE", false)]
    [InlineData("True", true)]
    [InlineData("False", false)]
    public void BooleanAdapter_Parse_CaseInsensitive_ShouldSucceed(string value, bool expected)
    {
        var adapter = new BooleanAdapter();
        var result = adapter.Parse(value);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void BooleanAdapter_Format_ShouldReturnLowercaseString(bool value, string expected)
    {
        var adapter = new BooleanAdapter();
        var result = adapter.Format(value);
        result.ShouldBe(expected);
    }
}

public class Base64AdapterTests
{
    [Fact]
    public void Base64Adapter_Parse_ValidBase64_ShouldSucceed()
    {
        var adapter = new Base64Adapter();
        var result = adapter.Parse("SGVsbG8gV29ybGQ="); // "Hello World"
        result.ShouldBe("Hello World"u8.ToArray());
    }

    [Theory]
    [InlineData("")]
    [InlineData("not valid base64!@#")]
    public void Base64Adapter_Parse_InvalidBase64_ShouldThrow(string value)
    {
        var adapter = new Base64Adapter();
        Should.Throw<DataTypeParseException>(() => adapter.Parse(value));
    }

    [Fact]
    public void Base64Adapter_Format_ShouldReturnBase64String()
    {
        var adapter = new Base64Adapter();
        var result = adapter.Format("Hello World"u8.ToArray());
        result.ShouldBe("SGVsbG8gV29ybGQ=");
    }

    [Fact]
    public void Base64Adapter_RoundTrip_ShouldPreserveValue()
    {
        var adapter = new Base64Adapter();
        var original = new byte[] { 1, 2, 3, 4, 5 };
        var formatted = adapter.Format(original);
        var parsed = adapter.Parse(formatted);
        parsed.ShouldBe(original);
    }
}
