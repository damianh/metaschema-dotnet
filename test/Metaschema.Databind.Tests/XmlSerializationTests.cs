using Metaschema.Core.Loading;
using Metaschema.Databind.Nodes;
using Shouldly;
using Xunit;

namespace Metaschema.Databind.Tests;

public class XmlSerializationTests
{
    private static string GetTestDataPath(string relativePath) =>
        Path.Combine(AppContext.BaseDirectory, "TestData", relativePath);

    private static BindingContext CreateContext()
    {
        var loader = new ModuleLoader();
        var module = loader.Load(GetTestDataPath("simple-module.xml"));
        return new BindingContext(module);
    }

    [Fact]
    public void Deserialize_SimpleXmlDocument_ShouldParseRootAssembly()
    {
        // Arrange
        var context = CreateContext();
        var deserializer = context.GetDeserializer(Format.Xml);
        var xml = """
                  <?xml version="1.0" encoding="UTF-8"?>
                  <document xmlns="http://example.com/ns/simple-test" id="doc-001" name="Test Document">
                    <title>Sample Title</title>
                  </document>
                  """;

        // Act
        var doc = deserializer.Deserialize(xml);

        // Assert
        doc.ShouldNotBeNull();
        doc.Name.ShouldBe("document");
        doc.RootAssembly.ShouldNotBeNull();
        doc.RootAssembly!.Definition.Name.ShouldBe("document");
    }

    [Fact]
    public void Deserialize_XmlWithFlags_ShouldParseAttributes()
    {
        // Arrange
        var context = CreateContext();
        var deserializer = context.GetDeserializer(Format.Xml);
        var xml = """
                  <?xml version="1.0" encoding="UTF-8"?>
                  <document xmlns="http://example.com/ns/simple-test" id="doc-001" name="Test Document">
                    <title>Sample Title</title>
                  </document>
                  """;

        // Act
        var doc = deserializer.Deserialize(xml);

        // Assert
        var flags = doc.RootAssembly!.Flags;
        flags.Count.ShouldBe(2);
        flags["id"].RawValue.ShouldBe("doc-001");
        flags["name"].RawValue.ShouldBe("Test Document");
    }

    [Fact]
    public void Deserialize_XmlWithFields_ShouldParseElements()
    {
        // Arrange
        var context = CreateContext();
        var deserializer = context.GetDeserializer(Format.Xml);
        var xml = """
                  <?xml version="1.0" encoding="UTF-8"?>
                  <document xmlns="http://example.com/ns/simple-test" id="doc-001">
                    <title>Sample Title</title>
                  </document>
                  """;

        // Act
        var doc = deserializer.Deserialize(xml);

        // Assert
        var children = doc.RootAssembly!.ModelChildren;
        children.Count.ShouldBe(1);
        var title = children[0].ShouldBeOfType<FieldNode>();
        title.Name.ShouldBe("title");
        title.RawValue.ShouldBe("Sample Title");
    }

    [Fact]
    public void Serialize_DocumentNode_ShouldProduceValidXml()
    {
        // Arrange
        var context = CreateContext();
        var rootDef = context.ResolveRootAssembly("document")!;
        var flagDef = rootDef.ContainingModule.GetFlagDefinition("id")!;
        var fieldDef = rootDef.ContainingModule.GetFieldDefinition("title")!;

        var doc = new DocumentNode("document", rootDef);
        var assembly = new AssemblyNode("document", rootDef, doc);
        var idFlag = new FlagNode("id", flagDef, assembly) { RawValue = "doc-001" };
        assembly.AddFlag(idFlag);
        var titleField = new FieldNode("title", fieldDef, assembly) { RawValue = "Test Title" };
        assembly.AddModelChild(titleField);
        doc.RootAssembly = assembly;
        doc.AddChild(assembly);

        var serializer = context.GetSerializer(Format.Xml);

        // Act
        var xml = serializer.SerializeToString(doc);

        // Assert
        xml.ShouldContain("<document");
        xml.ShouldContain("id=\"doc-001\"");
        xml.ShouldContain("<title>Test Title</title>");
    }

    [Fact]
    public void Roundtrip_XmlContent_ShouldPreserveData()
    {
        // Arrange
        var context = CreateContext();
        var originalXml = """
                          <?xml version="1.0" encoding="UTF-8"?>
                          <document xmlns="http://example.com/ns/simple-test" id="doc-001" name="Test">
                            <title>Sample Title</title>
                          </document>
                          """;

        // Act
        var deserializer = context.GetDeserializer(Format.Xml);
        var serializer = context.GetSerializer(Format.Xml);
        var doc = deserializer.Deserialize(originalXml);
        var outputXml = serializer.SerializeToString(doc);

        // Assert
        outputXml.ShouldContain("id=\"doc-001\"");
        outputXml.ShouldContain("name=\"Test\"");
        outputXml.ShouldContain("<title>Sample Title</title>");
    }
}
