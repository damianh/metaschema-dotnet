// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using Metaschema.Datatypes.Adapters;
using Metaschema.Markup;
using Shouldly;
using Xunit;

namespace Metaschema.Datatypes;

public class MarkupLineAdapterTests
{
    [Theory]
    [InlineData("Hello world")]
    [InlineData("Text with **bold** and *italic*")]
    [InlineData("Simple text")]
    public void MarkupLineAdapter_Parse_ValidMarkup_ShouldSucceed(string input)
    {
        var adapter = new MarkupLineAdapter();
        var result = adapter.Parse(input);
        result.Value.ShouldBe(input);
    }

    [Fact]
    public void MarkupLineAdapter_Parse_Null_ShouldThrow()
    {
        var adapter = new MarkupLineAdapter();
        Should.Throw<ArgumentNullException>(() => adapter.Parse(null!));
    }

    [Fact]
    public void MarkupLineAdapter_Format_ShouldReturnValue()
    {
        var adapter = new MarkupLineAdapter();
        var markup = new MarkupLine("Hello **world**");
        var result = adapter.Format(markup);
        result.ShouldBe("Hello **world**");
    }

    [Fact]
    public void MarkupLineAdapter_TryParse_ValidInput_ShouldReturnTrue()
    {
        var adapter = new MarkupLineAdapter();
        var success = adapter.TryParse("Hello", out var result);
        success.ShouldBeTrue();
        result.Value.ShouldBe("Hello");
    }

    [Fact]
    public void MarkupLineAdapter_TryParse_NullInput_ShouldReturnFalse()
    {
        var adapter = new MarkupLineAdapter();
        var success = adapter.TryParse(null!, out var result);
        success.ShouldBeFalse();
        result.ShouldBe(default(MarkupLine));
    }

    [Fact]
    public void MarkupLineAdapter_RoundTrip_ShouldPreserveValue()
    {
        var adapter = new MarkupLineAdapter();
        const string original = "Text with *emphasis*";
        var formatted = adapter.Format(adapter.Parse(original));
        formatted.ShouldBe(original);
    }

    [Fact]
    public void MarkupLineAdapter_Validate_ValidInput_ShouldReturnValid()
    {
        var adapter = new MarkupLineAdapter();
        var result = adapter.Validate("Some markup");
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void MarkupLineAdapter_Validate_Null_ShouldReturnInvalid()
    {
        var adapter = new MarkupLineAdapter();
        var result = adapter.Validate(null!);
        result.IsValid.ShouldBeFalse();
    }
}

public class MarkupMultilineAdapterTests
{
    [Theory]
    [InlineData("Hello world")]
    [InlineData("Line 1\nLine 2\nLine 3")]
    [InlineData("# Heading\n\nParagraph text")]
    public void MarkupMultilineAdapter_Parse_ValidMarkup_ShouldSucceed(string input)
    {
        var adapter = new MarkupMultilineAdapter();
        var result = adapter.Parse(input);
        result.Value.ShouldBe(input);
    }

    [Fact]
    public void MarkupMultilineAdapter_Parse_Null_ShouldThrow()
    {
        var adapter = new MarkupMultilineAdapter();
        Should.Throw<ArgumentNullException>(() => adapter.Parse(null!));
    }

    [Fact]
    public void MarkupMultilineAdapter_Format_ShouldReturnValue()
    {
        var adapter = new MarkupMultilineAdapter();
        var markup = new MarkupMultiline("# Heading\n\nParagraph");
        var result = adapter.Format(markup);
        result.ShouldBe("# Heading\n\nParagraph");
    }

    [Fact]
    public void MarkupMultilineAdapter_RoundTrip_ShouldPreserveValue()
    {
        var adapter = new MarkupMultilineAdapter();
        const string original = "# Title\n\n- Item 1\n- Item 2";
        var formatted = adapter.Format(adapter.Parse(original));
        formatted.ShouldBe(original);
    }
}
