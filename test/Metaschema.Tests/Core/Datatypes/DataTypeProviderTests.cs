// Licensed under the MIT License.

using Metaschema.Datatypes.Adapters;
using Shouldly;
using Xunit;

namespace Metaschema.Datatypes;

public class DataTypeProviderTests
{
    [Fact]
    public void Default_ShouldContainAllBuiltInAdapters()
    {
        // Arrange & Act
        var provider = DataTypeProvider.Default;
        var adapters = provider.GetAllAdapters().ToList();

        // Assert
        adapters.Count.ShouldBe(23);
    }

    [Theory]
    [InlineData(MetaschemaDataTypes.StringType)]
    [InlineData(MetaschemaDataTypes.Token)]
    [InlineData(MetaschemaDataTypes.Uri)]
    [InlineData(MetaschemaDataTypes.UriReference)]
    [InlineData(MetaschemaDataTypes.Uuid)]
    [InlineData(MetaschemaDataTypes.EmailAddress)]
    [InlineData(MetaschemaDataTypes.Hostname)]
    [InlineData(MetaschemaDataTypes.IntegerType)]
    [InlineData(MetaschemaDataTypes.NonNegativeInteger)]
    [InlineData(MetaschemaDataTypes.PositiveInteger)]
    [InlineData(MetaschemaDataTypes.DecimalType)]
    [InlineData(MetaschemaDataTypes.Boolean)]
    [InlineData(MetaschemaDataTypes.Base64)]
    [InlineData(MetaschemaDataTypes.Date)]
    [InlineData(MetaschemaDataTypes.DateWithTimezone)]
    [InlineData(MetaschemaDataTypes.DateTime)]
    [InlineData(MetaschemaDataTypes.DateTimeWithTimezone)]
    [InlineData(MetaschemaDataTypes.DayTimeDuration)]
    [InlineData(MetaschemaDataTypes.YearMonthDuration)]
    [InlineData(MetaschemaDataTypes.Ipv4Address)]
    [InlineData(MetaschemaDataTypes.Ipv6Address)]
    [InlineData(MetaschemaDataTypes.MarkupLine)]
    [InlineData(MetaschemaDataTypes.MarkupMultiline)]
    public void GetAdapter_ShouldReturnAdapterForAllBuiltInTypes(string typeName)
    {
        // Arrange
        var provider = DataTypeProvider.Default;

        // Act
        var adapter = provider.GetAdapter(typeName);

        // Assert
        adapter.ShouldNotBeNull();
        adapter.TypeName.ShouldBe(typeName);
    }

    [Fact]
    public void GetAdapter_UnknownType_ShouldReturnNull()
    {
        // Arrange
        var provider = DataTypeProvider.Default;

        // Act
        var adapter = provider.GetAdapter("unknown-type");

        // Assert
        adapter.ShouldBeNull();
    }

    [Fact]
    public void GetAdapterGeneric_ShouldReturnTypedAdapter()
    {
        // Arrange
        var provider = DataTypeProvider.Default;

        // Act
        var adapter = provider.GetAdapter<long>(MetaschemaDataTypes.IntegerType);

        // Assert
        adapter.ShouldNotBeNull();
        adapter.ShouldBeOfType<IntegerAdapter>();
    }

    [Fact]
    public void GetAdapterGeneric_WrongType_ShouldReturnNull()
    {
        // Arrange
        var provider = DataTypeProvider.Default;

        // Act - integer adapter holds long, not string
        var adapter = provider.GetAdapter<string>(MetaschemaDataTypes.IntegerType);

        // Assert
        adapter.ShouldBeNull();
    }

    [Fact]
    public void RegisterAdapter_ShouldOverrideExisting()
    {
        // Arrange
        var provider = new DataTypeProvider();
        var adapter1 = new StringAdapter();
        var adapter2 = new StringAdapter();

        // Act
        provider.RegisterAdapter(adapter1);
        provider.RegisterAdapter(adapter2);

        // Assert
        var result = provider.GetAdapter(MetaschemaDataTypes.StringType);
        result.ShouldBeSameAs(adapter2);
    }
}
