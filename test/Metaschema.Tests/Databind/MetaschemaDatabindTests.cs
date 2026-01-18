// Licensed under the MIT License.

using Metaschema.Core.Loading;
using Metaschema.Databind.Nodes;
using Shouldly;
using Xunit;

namespace Metaschema.Databind;

public class MetaschemaDatabindTests
{
    [Fact]
    public void Version_ShouldReturnValue()
    {
        var version = MetaschemaDatabind.Version;
        version.ShouldNotBeNullOrEmpty();
    }
}

public class JsonSerializationTests
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
    public void Deserialize_SimpleJsonDocument_ShouldParseRootAssembly()
    {
        // Arrange
        var context = CreateContext();
        var deserializer = context.GetDeserializer(Format.Json);
        var json = """
            {
              "document": {
                "id": "doc-001",
                "name": "Test Document",
                "title": "Sample Title"
              }
            }
            """;

        // Act
        var doc = deserializer.Deserialize(json);

        // Assert
        doc.ShouldNotBeNull();
        doc.Name.ShouldBe("document");
        doc.RootAssembly.ShouldNotBeNull();
    }

    [Fact]
    public void Deserialize_JsonWithFlags_ShouldParseProperties()
    {
        // Arrange
        var context = CreateContext();
        var deserializer = context.GetDeserializer(Format.Json);
        var json = """
            {
              "document": {
                "id": "doc-001",
                "name": "Test Document",
                "title": "Sample Title"
              }
            }
            """;

        // Act
        var doc = deserializer.Deserialize(json);

        // Assert
        var flags = doc.RootAssembly!.Flags;
        flags.Count.ShouldBe(2);
        flags["id"].RawValue.ShouldBe("doc-001");
        flags["name"].RawValue.ShouldBe("Test Document");
    }

    [Fact]
    public void Serialize_DocumentNode_ShouldProduceValidJson()
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

        var serializer = context.GetSerializer(Format.Json);

        // Act
        var json = serializer.SerializeToString(doc);

        // Assert
        json.ShouldContain("\"document\"");
        json.ShouldContain("\"id\": \"doc-001\"");
        json.ShouldContain("\"title\": \"Test Title\"");
    }

    [Fact]
    public void Roundtrip_JsonContent_ShouldPreserveData()
    {
        // Arrange
        var context = CreateContext();
        var originalJson = """
            {
              "document": {
                "id": "doc-001",
                "name": "Test",
                "title": "Sample Title"
              }
            }
            """;

        // Act
        var deserializer = context.GetDeserializer(Format.Json);
        var serializer = context.GetSerializer(Format.Json);
        var doc = deserializer.Deserialize(originalJson);
        var outputJson = serializer.SerializeToString(doc);

        // Assert
        outputJson.ShouldContain("\"id\": \"doc-001\"");
        outputJson.ShouldContain("\"name\": \"Test\"");
        outputJson.ShouldContain("\"title\": \"Sample Title\"");
    }
}

public class YamlSerializationTests
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
    public void Deserialize_SimpleYamlDocument_ShouldParseRootAssembly()
    {
        // Arrange
        var context = CreateContext();
        var deserializer = context.GetDeserializer(Format.Yaml);
        var yaml = """
            document:
              id: doc-001
              name: Test Document
              title: Sample Title
            """;

        // Act
        var doc = deserializer.Deserialize(yaml);

        // Assert
        doc.ShouldNotBeNull();
        doc.Name.ShouldBe("document");
        doc.RootAssembly.ShouldNotBeNull();
    }

    [Fact]
    public void Deserialize_YamlWithFlags_ShouldParseProperties()
    {
        // Arrange
        var context = CreateContext();
        var deserializer = context.GetDeserializer(Format.Yaml);
        var yaml = """
            document:
              id: doc-001
              name: Test Document
              title: Sample Title
            """;

        // Act
        var doc = deserializer.Deserialize(yaml);

        // Assert
        var flags = doc.RootAssembly!.Flags;
        flags.Count.ShouldBe(2);
        flags["id"].RawValue.ShouldBe("doc-001");
        flags["name"].RawValue.ShouldBe("Test Document");
    }

    [Fact]
    public void Serialize_DocumentNode_ShouldProduceValidYaml()
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

        var serializer = context.GetSerializer(Format.Yaml);

        // Act
        var yaml = serializer.SerializeToString(doc);

        // Assert
        yaml.ShouldContain("document:");
        yaml.ShouldContain("id: doc-001");
        yaml.ShouldContain("title: Test Title");
    }

    [Fact]
    public void Roundtrip_YamlContent_ShouldPreserveData()
    {
        // Arrange
        var context = CreateContext();
        var originalYaml = """
            document:
              id: doc-001
              name: Test
              title: Sample Title
            """;

        // Act
        var deserializer = context.GetDeserializer(Format.Yaml);
        var serializer = context.GetSerializer(Format.Yaml);
        var doc = deserializer.Deserialize(originalYaml);
        var outputYaml = serializer.SerializeToString(doc);

        // Assert
        outputYaml.ShouldContain("id: doc-001");
        outputYaml.ShouldContain("name: Test");
        outputYaml.ShouldContain("title: Sample Title");
    }
}

public class BoundLoaderTests
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
    public void DetectFormat_XmlContent_ShouldReturnXml()
    {
        // Arrange
        var context = CreateContext();
        var loader = context.NewBoundLoader();
        var xml = "<?xml version=\"1.0\"?><root/>";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml));

        // Act
        var format = loader.DetectFormat(stream);

        // Assert
        format.ShouldBe(Format.Xml);
    }

    [Fact]
    public void DetectFormat_JsonContent_ShouldReturnJson()
    {
        // Arrange
        var context = CreateContext();
        var loader = context.NewBoundLoader();
        var json = "{\"key\": \"value\"}";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));

        // Act
        var format = loader.DetectFormat(stream);

        // Assert
        format.ShouldBe(Format.Json);
    }

    [Fact]
    public void DetectFormat_YamlContent_ShouldReturnYaml()
    {
        // Arrange
        var context = CreateContext();
        var loader = context.NewBoundLoader();
        var yaml = "key: value";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(yaml));

        // Act
        var format = loader.DetectFormat(stream);

        // Assert
        format.ShouldBe(Format.Yaml);
    }

    [Fact]
    public void DetectFormat_YamlWithDocumentMarker_ShouldReturnYaml()
    {
        // Arrange
        var context = CreateContext();
        var loader = context.NewBoundLoader();
        var yaml = "---\nkey: value";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(yaml));

        // Act
        var format = loader.DetectFormat(stream);

        // Assert
        format.ShouldBe(Format.Yaml);
    }
}

public class FormatConversionTests
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
    public void ConvertXmlToJson_ShouldProduceEquivalentDocument()
    {
        // Arrange
        var context = CreateContext();
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <document xmlns="http://example.com/ns/simple-test" id="doc-001" name="Test">
              <title>Sample Title</title>
            </document>
            """;

        // Act
        var xmlDeserializer = context.GetDeserializer(Format.Xml);
        var jsonSerializer = context.GetSerializer(Format.Json);
        var doc = xmlDeserializer.Deserialize(xml);
        var json = jsonSerializer.SerializeToString(doc);

        // Assert
        json.ShouldContain("\"document\"");
        json.ShouldContain("\"id\": \"doc-001\"");
        json.ShouldContain("\"name\": \"Test\"");
        json.ShouldContain("\"title\": \"Sample Title\"");
    }

    [Fact]
    public void ConvertJsonToYaml_ShouldProduceEquivalentDocument()
    {
        // Arrange
        var context = CreateContext();
        var json = """
            {
              "document": {
                "id": "doc-001",
                "name": "Test",
                "title": "Sample Title"
              }
            }
            """;

        // Act
        var jsonDeserializer = context.GetDeserializer(Format.Json);
        var yamlSerializer = context.GetSerializer(Format.Yaml);
        var doc = jsonDeserializer.Deserialize(json);
        var yaml = yamlSerializer.SerializeToString(doc);

        // Assert
        yaml.ShouldContain("document:");
        yaml.ShouldContain("id: doc-001");
        yaml.ShouldContain("name: Test");
        yaml.ShouldContain("title: Sample Title");
    }

    [Fact]
    public void ConvertYamlToXml_ShouldProduceEquivalentDocument()
    {
        // Arrange
        var context = CreateContext();
        var yaml = """
            document:
              id: doc-001
              name: Test
              title: Sample Title
            """;

        // Act
        var yamlDeserializer = context.GetDeserializer(Format.Yaml);
        var xmlSerializer = context.GetSerializer(Format.Xml);
        var doc = yamlDeserializer.Deserialize(yaml);
        var xml = xmlSerializer.SerializeToString(doc);

        // Assert
        xml.ShouldContain("<document");
        xml.ShouldContain("id=\"doc-001\"");
        xml.ShouldContain("name=\"Test\"");
        xml.ShouldContain("<title>Sample Title</title>");
    }
}
