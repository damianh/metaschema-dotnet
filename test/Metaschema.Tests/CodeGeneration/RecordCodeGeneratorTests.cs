// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using Metaschema.CodeGeneration;
using Metaschema.Loading;
using Shouldly;
using Xunit;

namespace Metaschema.Tests.CodeGeneration;

public class RecordCodeGeneratorTests
{
    [Fact]
    public void Generate_ProducesRecordsWithJsonAttributes()
    {
        // Arrange
        var loader = new ModuleLoader();
        var metaschemaPath = Path.Combine(AppContext.BaseDirectory, "TestData", "oscal_metadata_metaschema.xml");

        // Skip if file doesn't exist (test data not available)
        if (!File.Exists(metaschemaPath))
        {
            return;
        }

        var module = loader.Load(metaschemaPath);
        var generator = new RecordCodeGenerator(new CodeGenerationOptions
        {
            Namespace = "Oscal.Metadata",
            UseRecords = true,
            GenerateJsonContext = true,
            GenerateExtensionMethods = true,
            FilePerType = true
        });

        // Act
        var files = generator.Generate(module);

        // Assert
        files.ShouldNotBeEmpty();
        files.ShouldContainKey("Metadata.g.cs");
        files.ShouldContainKey("MetadataJsonContext.g.cs");
        files.ShouldContainKey("Extensions.g.cs");

        // Check that Metadata record has proper structure
        var metadataFile = files["Metadata.g.cs"];
        metadataFile.ShouldContain("public sealed record Metadata");
        metadataFile.ShouldContain("[JsonPropertyName(");
        metadataFile.ShouldContain("{ get; init; }");
        metadataFile.ShouldNotContain("[MetaschemaAssembly");  // No attributes needed
        metadataFile.ShouldNotContain("[BoundField");  // No attributes needed

        // Check JsonContext
        var contextFile = files["MetadataJsonContext.g.cs"];
        contextFile.ShouldContain("public partial class MetadataJsonContext : JsonSerializerContext");
        contextFile.ShouldContain("[JsonSerializable(typeof(Metadata))]");
        contextFile.ShouldContain("[JsonSourceGenerationOptions(");

        // Check Extensions
        var extensionsFile = files["Extensions.g.cs"];
        extensionsFile.ShouldContain("public static class Extensions");
        extensionsFile.ShouldContain("LoadFromJson");
        extensionsFile.ShouldContain("SaveToJson");
    }

    [Fact]
    public void Generate_CreatesImmutableRecordsWithInitProperties()
    {
        // Arrange
        var loader = new ModuleLoader();
        var metaschemaPath = Path.Combine(AppContext.BaseDirectory, "TestData", "oscal_metadata_metaschema.xml");

        if (!File.Exists(metaschemaPath))
        {
            return;
        }

        var module = loader.Load(metaschemaPath);
        var generator = new RecordCodeGenerator();

        // Act
        var files = generator.Generate(module);

        // Assert - all properties should use init-only setters
        foreach (var (fileName, content) in files)
        {
            if (fileName.EndsWith(".g.cs", StringComparison.Ordinal) && !fileName.Contains("Context") && !fileName.Contains("Extensions"))
            {
                content.ShouldMatch(@"public.*\{ get; init; \}");
            }
        }
    }
}
