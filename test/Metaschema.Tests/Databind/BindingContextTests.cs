using Metaschema.Core.Loading;
using Metaschema.Databind.Serialization;
using Shouldly;
using Xunit;

namespace Metaschema.Databind;

public class BindingContextTests
{
    private static string GetTestDataPath(string relativePath) =>
        Path.Combine(AppContext.BaseDirectory, "TestData", relativePath);

    [Fact]
    public void RegisterModule_ShouldIndexRootAssemblies()
    {
        // Arrange
        var loader = new ModuleLoader();
        var module = loader.Load(GetTestDataPath("simple-module.xml"));
        var context = new BindingContext();

        // Act
        context.RegisterModule(module);

        // Assert
        var resolved = context.ResolveRootAssembly("document");
        resolved.ShouldNotBeNull();
        resolved.Name.ShouldBe("document");
    }

    [Fact]
    public void ResolveRootAssembly_WithNamespace_ShouldFindAssembly()
    {
        // Arrange
        var loader = new ModuleLoader();
        var module = loader.Load(GetTestDataPath("simple-module.xml"));
        var context = new BindingContext(module);

        // Act
        var resolved = context.ResolveRootAssembly("document", new Uri("http://example.com/ns/simple-test"));

        // Assert
        resolved.ShouldNotBeNull();
        resolved.Name.ShouldBe("document");
    }

    [Fact]
    public void GetSerializer_ShouldReturnCorrectType()
    {
        // Arrange
        var loader = new ModuleLoader();
        var module = loader.Load(GetTestDataPath("simple-module.xml"));
        var context = new BindingContext(module);

        // Act & Assert
        context.GetSerializer(Format.Xml).ShouldBeOfType<XmlContentSerializer>();
        context.GetSerializer(Format.Json).ShouldBeOfType<JsonContentSerializer>();
        context.GetSerializer(Format.Yaml).ShouldBeOfType<YamlContentSerializer>();
    }

    [Fact]
    public void GetDeserializer_ShouldReturnCorrectType()
    {
        // Arrange
        var loader = new ModuleLoader();
        var module = loader.Load(GetTestDataPath("simple-module.xml"));
        var context = new BindingContext(module);

        // Act & Assert
        context.GetDeserializer(Format.Xml).ShouldBeOfType<XmlContentDeserializer>();
        context.GetDeserializer(Format.Json).ShouldBeOfType<JsonContentDeserializer>();
        context.GetDeserializer(Format.Yaml).ShouldBeOfType<YamlContentDeserializer>();
    }
}
