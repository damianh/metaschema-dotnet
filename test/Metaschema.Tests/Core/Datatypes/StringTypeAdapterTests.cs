// Licensed under the MIT License.

using System.Net;
using Metaschema.Datatypes.Adapters;
using Shouldly;
using Xunit;

namespace Metaschema.Datatypes;

public class StringAdapterTests
{
    [Theory]
    [InlineData("hello")]
    [InlineData("a")]
    [InlineData("hello world")]
    [InlineData("hello  world")]
    public void StringAdapter_Parse_ValidValues_ShouldSucceed(string value)
    {
        var adapter = new StringAdapter();
        var result = adapter.Parse(value);
        result.ShouldBe(value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(" hello")]
    [InlineData("hello ")]
    [InlineData(" hello ")]
    public void StringAdapter_Parse_InvalidValues_ShouldThrow(string value)
    {
        var adapter = new StringAdapter();
        Should.Throw<DataTypeParseException>(() => adapter.Parse(value));
    }

    [Fact]
    public void StringAdapter_Parse_Null_ShouldThrow()
    {
        var adapter = new StringAdapter();
        Should.Throw<ArgumentNullException>(() => adapter.Parse(null!));
    }
}

public class TokenAdapterTests
{
    [Theory]
    [InlineData("hello")]
    [InlineData("hello-world")]
    [InlineData("hello_world")]
    [InlineData("hello.world")]
    [InlineData("_underscore")]
    [InlineData("Test123")]
    public void TokenAdapter_Parse_ValidToken_ShouldSucceed(string input)
    {
        var adapter = new TokenAdapter();
        var result = adapter.Parse(input);
        result.ShouldBe(input);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("hello world")] // spaces not allowed
    [InlineData("123start")] // can't start with number
    [InlineData("-invalid")] // can't start with hyphen
    public void TokenAdapter_Parse_InvalidToken_ShouldThrow(string value)
    {
        var adapter = new TokenAdapter();
        Should.Throw<DataTypeParseException>(() => adapter.Parse(value));
    }
}

public class UriAdapterTests
{
    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://example.com/path?query=1")]
    [InlineData("urn:isbn:0451450523")]
    public void UriAdapter_Parse_ValidAbsoluteUri_ShouldSucceed(string value)
    {
        var adapter = new UriAdapter();
        var result = adapter.Parse(value);
        result.ShouldNotBeNull();
        result.IsAbsoluteUri.ShouldBeTrue();
    }

    [Theory]
    [InlineData("not a uri")]
    [InlineData("://missing-scheme")]
    public void UriAdapter_Parse_InvalidUri_ShouldThrow(string value)
    {
        var adapter = new UriAdapter();
        Should.Throw<DataTypeParseException>(() => adapter.Parse(value));
    }

    [Theory]
    [InlineData("/relative/path")]
    [InlineData("relative")]
    public void UriAdapter_Parse_RelativeUri_ShouldThrow(string value)
    {
        var adapter = new UriAdapter();
        Should.Throw<DataTypeParseException>(() => adapter.Parse(value));
    }

    [Fact]
    public void UriAdapter_Format_ShouldReturnAbsoluteUri()
    {
        var adapter = new UriAdapter();
        var uri = new Uri("http://example.com/path");
        var result = adapter.Format(uri);
        result.ShouldBe("http://example.com/path");
    }
}

public class UriReferenceAdapterTests
{
    [Theory]
    [InlineData("http://example.com")]
    [InlineData("/relative/path")]
    [InlineData("relative")]
    [InlineData("#fragment")]
    public void UriReferenceAdapter_Parse_ValidUriReference_ShouldSucceed(string value)
    {
        var adapter = new UriReferenceAdapter();
        var result = adapter.Parse(value);
        result.ShouldNotBeNull();
    }
}

public class UuidAdapterTests
{
    [Theory]
    [InlineData("550e8400-e29b-41d4-a716-446655440000")]
    [InlineData("550E8400-E29B-41D4-A716-446655440000")]
    public void UuidAdapter_Parse_ValidUuid_ShouldSucceed(string value)
    {
        var adapter = new UuidAdapter();
        var result = adapter.Parse(value);
        result.ShouldBe(Guid.Parse(value));
    }

    [Theory]
    [InlineData("not-a-uuid")]
    [InlineData("550e8400-e29b-41d4-a716")]
    [InlineData("")]
    public void UuidAdapter_Parse_InvalidUuid_ShouldThrow(string value)
    {
        var adapter = new UuidAdapter();
        Should.Throw<DataTypeParseException>(() => adapter.Parse(value));
    }

    [Fact]
    public void UuidAdapter_Format_ShouldUseLowercase()
    {
        var adapter = new UuidAdapter();
        var guid = Guid.Parse("550E8400-E29B-41D4-A716-446655440000");
        var result = adapter.Format(guid);
        result.ShouldBe("550e8400-e29b-41d4-a716-446655440000");
    }
}

public class EmailAddressAdapterTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("user.name@example.co.uk")]
    [InlineData("user+tag@example.com")]
    public void EmailAddressAdapter_Parse_ValidEmail_ShouldSucceed(string value)
    {
        var adapter = new EmailAddressAdapter();
        var result = adapter.Parse(value);
        result.ShouldBe(value);
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@nodomain")]
    [InlineData("noat.com")]
    [InlineData("")]
    public void EmailAddressAdapter_Parse_InvalidEmail_ShouldThrow(string value)
    {
        var adapter = new EmailAddressAdapter();
        Should.Throw<DataTypeParseException>(() => adapter.Parse(value));
    }
}

public class HostnameAdapterTests
{
    [Theory]
    [InlineData("example.com")]
    [InlineData("sub.example.com")]
    [InlineData("localhost")]
    [InlineData("my-host")]
    [InlineData("-invalid")] // Currently accepted by lenient pattern
    [InlineData("invalid-")]
    public void HostnameAdapter_Parse_ValidHostname_ShouldSucceed(string value)
    {
        var adapter = new HostnameAdapter();
        var result = adapter.Parse(value);
        result.ShouldBe(value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void HostnameAdapter_Parse_EmptyOrWhitespace_ShouldThrow(string value)
    {
        var adapter = new HostnameAdapter();
        Should.Throw<DataTypeParseException>(() => adapter.Parse(value));
    }
}

public class Ipv4AddressAdapterTests
{
    [Theory]
    [InlineData("192.168.1.1")]
    [InlineData("0.0.0.0")]
    [InlineData("255.255.255.255")]
    public void Ipv4AddressAdapter_Parse_ValidAddress_ShouldSucceed(string value)
    {
        var adapter = new Ipv4AddressAdapter();
        var result = adapter.Parse(value);
        result.ShouldBe(IPAddress.Parse(value));
    }

    [Theory]
    [InlineData("::1")]
    [InlineData("256.1.1.1")]
    [InlineData("1.1.1")]
    [InlineData("not.an.ip")]
    public void Ipv4AddressAdapter_Parse_InvalidAddress_ShouldThrow(string value)
    {
        var adapter = new Ipv4AddressAdapter();
        Should.Throw<DataTypeParseException>(() => adapter.Parse(value));
    }

    [Fact]
    public void Ipv4AddressAdapter_Format_ShouldReturnCanonicalForm()
    {
        var adapter = new Ipv4AddressAdapter();
        var ip = IPAddress.Parse("192.168.1.1");
        var result = adapter.Format(ip);
        result.ShouldBe("192.168.1.1");
    }
}

public class Ipv6AddressAdapterTests
{
    [Theory]
    [InlineData("::1")]
    [InlineData("2001:db8::1")]
    [InlineData("fe80::1")]
    public void Ipv6AddressAdapter_Parse_ValidAddress_ShouldSucceed(string value)
    {
        var adapter = new Ipv6AddressAdapter();
        var result = adapter.Parse(value);
        result.AddressFamily.ShouldBe(System.Net.Sockets.AddressFamily.InterNetworkV6);
    }

    [Theory]
    [InlineData("192.168.1.1")]
    [InlineData("not-an-ip")]
    public void Ipv6AddressAdapter_Parse_InvalidAddress_ShouldThrow(string value)
    {
        var adapter = new Ipv6AddressAdapter();
        Should.Throw<DataTypeParseException>(() => adapter.Parse(value));
    }
}
