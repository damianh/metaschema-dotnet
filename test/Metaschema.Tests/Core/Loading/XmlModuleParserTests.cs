// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using System.Xml.Linq;
using Metaschema.Constraints;
using Shouldly;
using Xunit;

namespace Metaschema.Loading;

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
        flag.Scope.ShouldBe(Metaschema.Model.Scope.Global); // default
    }

    #region Constraint Parsing Tests

    [Fact]
    public void Parse_AllowedValuesConstraint_ShouldParseCorrectly()
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
              <define-flag name="priority">
                <constraint>
                  <allowed-values id="priority-values" level="ERROR" allow-other="no">
                    <enum value="high">High priority</enum>
                    <enum value="medium">Medium priority</enum>
                    <enum value="low" deprecated="1.0.0">Low priority (deprecated)</enum>
                  </allowed-values>
                </constraint>
              </define-flag>
            </METASCHEMA>
            """;
        var doc = XDocument.Parse(xml);
        var parser = new XmlModuleParser(_ => throw new NotSupportedException());
        var uri = new Uri("file:///test.xml");

        // Act
        var module = parser.Parse(doc, uri);

        // Assert
        var flag = module.GetFlagDefinition("priority");
        flag.ShouldNotBeNull();
        flag.Constraints.Count.ShouldBe(1);

        var constraint = flag.Constraints[0].ShouldBeOfType<AllowedValuesConstraint>();
        constraint.Id.ShouldBe("priority-values");
        constraint.Level.ShouldBe(ConstraintLevel.Error);
        constraint.AllowOther.ShouldBeFalse();
        constraint.AllowedValues.Count.ShouldBe(3);
        constraint.AllowedValues[0].Value.ShouldBe("high");
        constraint.AllowedValues[0].Description.ShouldBe("High priority");
        constraint.AllowedValues[2].DeprecatedVersion.ShouldBe("1.0.0");
    }

    [Fact]
    public void Parse_MatchesConstraint_ShouldParseCorrectly()
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
              <define-field name="email">
                <constraint>
                  <matches id="email-pattern" regex="^[^@]+@[^@]+\.[^@]+$" target="."/>
                </constraint>
              </define-field>
            </METASCHEMA>
            """;
        var doc = XDocument.Parse(xml);
        var parser = new XmlModuleParser(_ => throw new NotSupportedException());
        var uri = new Uri("file:///test.xml");

        // Act
        var module = parser.Parse(doc, uri);

        // Assert
        var field = module.GetFieldDefinition("email");
        field.ShouldNotBeNull();
        field.Constraints.Count.ShouldBe(1);

        var constraint = field.Constraints[0].ShouldBeOfType<MatchesConstraint>();
        constraint.Id.ShouldBe("email-pattern");
        constraint.Pattern.ShouldBe("^[^@]+@[^@]+\\.[^@]+$");
        constraint.Target.ShouldBe(".");
    }

    [Fact]
    public void Parse_ExpectConstraint_ShouldParseCorrectly()
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
              <define-assembly name="item">
                <constraint>
                  <expect id="has-title" test="title" level="WARNING">
                    <message>Items should have a title</message>
                  </expect>
                </constraint>
              </define-assembly>
            </METASCHEMA>
            """;
        var doc = XDocument.Parse(xml);
        var parser = new XmlModuleParser(_ => throw new NotSupportedException());
        var uri = new Uri("file:///test.xml");

        // Act
        var module = parser.Parse(doc, uri);

        // Assert
        var assembly = module.GetAssemblyDefinition("item");
        assembly.ShouldNotBeNull();
        assembly.Constraints.Count.ShouldBe(1);

        var constraint = assembly.Constraints[0].ShouldBeOfType<ExpectConstraint>();
        constraint.Id.ShouldBe("has-title");
        constraint.Test.ShouldBe("title");
        constraint.Level.ShouldBe(ConstraintLevel.Warning);
        constraint.Message.ShouldBe("Items should have a title");
    }

    [Fact]
    public void Parse_IndexConstraint_ShouldParseCorrectly()
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
              <define-assembly name="catalog">
                <constraint>
                  <index name="controls-by-id" target="control">
                    <key-field target="@id"/>
                  </index>
                </constraint>
              </define-assembly>
            </METASCHEMA>
            """;
        var doc = XDocument.Parse(xml);
        var parser = new XmlModuleParser(_ => throw new NotSupportedException());
        var uri = new Uri("file:///test.xml");

        // Act
        var module = parser.Parse(doc, uri);

        // Assert
        var assembly = module.GetAssemblyDefinition("catalog");
        assembly.ShouldNotBeNull();
        assembly.Constraints.Count.ShouldBe(1);

        var constraint = assembly.Constraints[0].ShouldBeOfType<IndexConstraint>();
        constraint.Name.ShouldBe("controls-by-id");
        constraint.Target.ShouldBe("control");
        constraint.KeyFields.Count.ShouldBe(1);
        constraint.KeyFields[0].Target.ShouldBe("@id");
    }

    [Fact]
    public void Parse_CardinalityConstraint_ShouldParseCorrectly()
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
              <define-assembly name="group">
                <constraint>
                  <has-cardinality target="item" min-occurs="1" max-occurs="10"/>
                </constraint>
              </define-assembly>
            </METASCHEMA>
            """;
        var doc = XDocument.Parse(xml);
        var parser = new XmlModuleParser(_ => throw new NotSupportedException());
        var uri = new Uri("file:///test.xml");

        // Act
        var module = parser.Parse(doc, uri);

        // Assert
        var assembly = module.GetAssemblyDefinition("group");
        assembly.ShouldNotBeNull();
        assembly.Constraints.Count.ShouldBe(1);

        var constraint = assembly.Constraints[0].ShouldBeOfType<CardinalityConstraint>();
        constraint.Target.ShouldBe("item");
        constraint.MinOccurs.ShouldBe(1);
        constraint.MaxOccurs.ShouldBe(10);
    }

    [Fact]
    public void Parse_MultipleConstraints_ShouldParseAll()
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
              <define-field name="identifier">
                <constraint>
                  <matches regex="^[a-z][a-z0-9-]*$"/>
                  <allowed-values allow-other="yes">
                    <enum value="system">System identifier</enum>
                  </allowed-values>
                  <expect test="string-length(.) le 100"/>
                </constraint>
              </define-field>
            </METASCHEMA>
            """;
        var doc = XDocument.Parse(xml);
        var parser = new XmlModuleParser(_ => throw new NotSupportedException());
        var uri = new Uri("file:///test.xml");

        // Act
        var module = parser.Parse(doc, uri);

        // Assert
        var field = module.GetFieldDefinition("identifier");
        field.ShouldNotBeNull();
        field.Constraints.Count.ShouldBe(3);
        field.Constraints[0].ShouldBeOfType<MatchesConstraint>();
        field.Constraints[1].ShouldBeOfType<AllowedValuesConstraint>();
        field.Constraints[2].ShouldBeOfType<ExpectConstraint>();
    }

    [Fact]
    public void Parse_ConstraintLevel_ShouldParseAllLevels()
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
              <define-field name="test">
                <constraint>
                  <expect test="true()" level="CRITICAL"/>
                  <expect test="true()" level="ERROR"/>
                  <expect test="true()" level="WARNING"/>
                  <expect test="true()" level="INFORMATIONAL"/>
                </constraint>
              </define-field>
            </METASCHEMA>
            """;
        var doc = XDocument.Parse(xml);
        var parser = new XmlModuleParser(_ => throw new NotSupportedException());
        var uri = new Uri("file:///test.xml");

        // Act
        var module = parser.Parse(doc, uri);

        // Assert
        var field = module.GetFieldDefinition("test");
        field.ShouldNotBeNull();
        field.Constraints.Count.ShouldBe(4);
        field.Constraints[0].Level.ShouldBe(ConstraintLevel.Critical);
        field.Constraints[1].Level.ShouldBe(ConstraintLevel.Error);
        field.Constraints[2].Level.ShouldBe(ConstraintLevel.Warning);
        field.Constraints[3].Level.ShouldBe(ConstraintLevel.Informational);
    }

    #endregion

    #region Inline Flag Definition Tests

    [Fact]
    public void Parse_FieldWithInlineFlag_ShouldParseFlagInstance()
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
              <define-field name="hash">
                <formal-name>Hash</formal-name>
                <description>A cryptographic hash</description>
                <define-flag name="algorithm" as-type="string" required="yes">
                  <formal-name>Hash Algorithm</formal-name>
                  <description>The algorithm used</description>
                </define-flag>
              </define-field>
            </METASCHEMA>
            """;
        var doc = XDocument.Parse(xml);
        var parser = new XmlModuleParser(_ => throw new NotSupportedException());
        var uri = new Uri("file:///test.xml");

        // Act
        var module = parser.Parse(doc, uri);

        // Assert
        var field = module.GetFieldDefinition("hash");
        field.ShouldNotBeNull();
        field.FlagInstances.Count.ShouldBe(1);

        var flagInstance = field.FlagInstances[0];
        flagInstance.Ref.ShouldBe("algorithm");
        flagInstance.IsRequired.ShouldBeTrue();

        // The inline flag should have its definition attached
        flagInstance.ResolvedDefinition.ShouldNotBeNull();
        flagInstance.ResolvedDefinition!.Name.ShouldBe("algorithm");
        flagInstance.ResolvedDefinition.FormalName.ShouldBe("Hash Algorithm");
        flagInstance.ResolvedDefinition.Description!.ToString().ShouldBe("The algorithm used");
        flagInstance.ResolvedDefinition.DataTypeName.ShouldBe("string");
    }

    [Fact]
    public void Parse_AssemblyWithInlineFlag_ShouldParseFlagInstance()
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
              <define-assembly name="item">
                <define-flag name="id" as-type="token" required="yes">
                  <formal-name>Item ID</formal-name>
                  <description>Unique identifier</description>
                </define-flag>
                <model>
                  <define-field name="value"/>
                </model>
              </define-assembly>
            </METASCHEMA>
            """;
        var doc = XDocument.Parse(xml);
        var parser = new XmlModuleParser(_ => throw new NotSupportedException());
        var uri = new Uri("file:///test.xml");

        // Act
        var module = parser.Parse(doc, uri);

        // Assert
        var assembly = module.GetAssemblyDefinition("item");
        assembly.ShouldNotBeNull();
        assembly.FlagInstances.Count.ShouldBe(1);

        var flagInstance = assembly.FlagInstances[0];
        flagInstance.Ref.ShouldBe("id");
        flagInstance.IsRequired.ShouldBeTrue();
        flagInstance.ResolvedDefinition.ShouldNotBeNull();
        flagInstance.ResolvedDefinition!.Name.ShouldBe("id");
        flagInstance.ResolvedDefinition.DataTypeName.ShouldBe("token");
    }

    [Fact]
    public void Parse_FieldWithBothReferencedAndInlineFlags_ShouldParseBoth()
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
              <define-flag name="global-flag">
                <formal-name>Global Flag</formal-name>
              </define-flag>
              <define-field name="mixed">
                <flag ref="global-flag"/>
                <define-flag name="local-flag" required="yes">
                  <formal-name>Local Flag</formal-name>
                </define-flag>
              </define-field>
            </METASCHEMA>
            """;
        var doc = XDocument.Parse(xml);
        var parser = new XmlModuleParser(_ => throw new NotSupportedException());
        var uri = new Uri("file:///test.xml");

        // Act
        var module = parser.Parse(doc, uri);

        // Assert
        var field = module.GetFieldDefinition("mixed");
        field.ShouldNotBeNull();
        field.FlagInstances.Count.ShouldBe(2);

        // First flag is a reference
        field.FlagInstances[0].Ref.ShouldBe("global-flag");
        field.FlagInstances[0].IsRequired.ShouldBeFalse();

        // Second flag is inline
        field.FlagInstances[1].Ref.ShouldBe("local-flag");
        field.FlagInstances[1].IsRequired.ShouldBeTrue();
        field.FlagInstances[1].ResolvedDefinition.ShouldNotBeNull();
    }

    #endregion
}
