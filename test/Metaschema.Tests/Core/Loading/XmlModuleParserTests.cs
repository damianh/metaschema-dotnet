// Licensed under the MIT License.

using System.Xml.Linq;
using Shouldly;
using Xunit;

namespace Metaschema.Core.Loading;

public class XmlModuleParserTests
{
    [Fact]
    public void Parse_InvalidRootElement_ShouldThrowModuleLoadException()
    {
        // Arrange
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <NotAMetaschema xmlns="http://csrc.nist.gov/ns/oscal/metaschema/1.0">
            </NotAMetaschema>
            """;
        var doc = XDocument.Parse(xml);
        var parser = new XmlModuleParser(_ => throw new NotSupportedException());
        var uri = new Uri("file:///test.xml");

        // Act & Assert
        var exception = Should.Throw<ModuleLoadException>(() => parser.Parse(doc, uri));
        exception.Message.ShouldContain("METASCHEMA");
    }

    [Fact]
    public void Parse_MissingRequiredElement_ShouldThrowModuleLoadException()
    {
        // Arrange - missing schema-version
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <METASCHEMA xmlns="http://csrc.nist.gov/ns/oscal/metaschema/1.0">
              <schema-name>Test</schema-name>
              <short-name>test</short-name>
              <namespace>http://example.com/ns/test</namespace>
              <json-base-uri>http://example.com/schema/test</json-base-uri>
            </METASCHEMA>
            """;
        var doc = XDocument.Parse(xml);
        var parser = new XmlModuleParser(_ => throw new NotSupportedException());
        var uri = new Uri("file:///test.xml");

        // Act & Assert
        var exception = Should.Throw<ModuleLoadException>(() => parser.Parse(doc, uri));
        exception.Message.ShouldContain("schema-version");
    }

    [Fact]
    public void Parse_MissingImportHref_ShouldThrowModuleLoadException()
    {
        // Arrange
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <METASCHEMA xmlns="http://csrc.nist.gov/ns/oscal/metaschema/1.0">
              <schema-name>Test</schema-name>
              <short-name>test</short-name>
              <schema-version>1.0.0</schema-version>
              <namespace>http://example.com/ns/test</namespace>
              <json-base-uri>http://example.com/schema/test</json-base-uri>
              <import/>
            </METASCHEMA>
            """;
        var doc = XDocument.Parse(xml);
        var parser = new XmlModuleParser(_ => throw new NotSupportedException());
        var uri = new Uri("file:///test.xml");

        // Act & Assert
        var exception = Should.Throw<ModuleLoadException>(() => parser.Parse(doc, uri));
        exception.Message.ShouldContain("href");
    }

    [Fact]
    public void Parse_MinimalValidModule_ShouldSucceed()
    {
        // Arrange
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <METASCHEMA xmlns="http://csrc.nist.gov/ns/oscal/metaschema/1.0">
              <schema-name>Minimal</schema-name>
              <short-name>minimal</short-name>
              <schema-version>1.0.0</schema-version>
              <namespace>http://example.com/ns/minimal</namespace>
              <json-base-uri>http://example.com/schema/minimal</json-base-uri>
            </METASCHEMA>
            """;
        var doc = XDocument.Parse(xml);
        var parser = new XmlModuleParser(_ => throw new NotSupportedException());
        var uri = new Uri("file:///test.xml");

        // Act
        var module = parser.Parse(doc, uri);

        // Assert
        module.ShouldNotBeNull();
        module.Name.ShouldBe("Minimal");
        module.ShortName.ShouldBe("minimal");
        module.Version.ShouldBe("1.0.0");
        module.FlagDefinitions.ShouldBeEmpty();
        module.FieldDefinitions.ShouldBeEmpty();
        module.AssemblyDefinitions.ShouldBeEmpty();
    }

    [Fact]
    public void Parse_FlagWithDefaults_ShouldUseDefaults()
    {
        // Arrange
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <METASCHEMA xmlns="http://csrc.nist.gov/ns/oscal/metaschema/1.0">
              <schema-name>Test</schema-name>
              <short-name>test</short-name>
              <schema-version>1.0.0</schema-version>
              <namespace>http://example.com/ns/test</namespace>
              <json-base-uri>http://example.com/schema/test</json-base-uri>
              <define-flag name="my-flag"/>
            </METASCHEMA>
            """;
        var doc = XDocument.Parse(xml);
        var parser = new XmlModuleParser(_ => throw new NotSupportedException());
        var uri = new Uri("file:///test.xml");

        // Act
        var module = parser.Parse(doc, uri);

        // Assert
        var flag = module.GetFlagDefinition("my-flag");
        flag.ShouldNotBeNull();
        flag.DataTypeName.ShouldBe("string"); // default
        flag.Scope.ShouldBe(Metaschema.Core.Model.Scope.Global); // default
    }
}
