// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using Metaschema.Model;
using Shouldly;
using Xunit;

namespace Metaschema.Loading;

public class ModuleLoaderTests
{
    private static string GetTestDataPath(string relativePath) =>
        Path.Combine(AppContext.BaseDirectory, "TestData", relativePath);

    [Fact]
    public void Load_SimpleModule_ShouldParseHeaderCorrectly()
    {
        // Arrange
        var loader = new ModuleLoader();
        var path = GetTestDataPath("simple-module.xml");

        // Act
        var module = loader.Load(path);

        // Assert
        module.ShouldNotBeNull();
        module.Name.ShouldBe("Simple Test Module");
        module.ShortName.ShouldBe("simple-test");
        module.Version.ShouldBe("1.0.0");
        module.XmlNamespace.ToString().ShouldBe("http://example.com/ns/simple-test");
        module.JsonBaseUri.ToString().ShouldBe("http://example.com/schema/simple-test");
        module.Remarks.ShouldNotBeNull();
    }

    [Fact]
    public void Load_SimpleModule_ShouldParseFlagDefinitions()
    {
        // Arrange
        var loader = new ModuleLoader();
        var path = GetTestDataPath("simple-module.xml");

        // Act
        var module = loader.Load(path);

        // Assert
        var flags = module.FlagDefinitions.ToList();
        flags.Count.ShouldBe(2);

        var idFlag = module.GetFlagDefinition("id");
        idFlag.ShouldNotBeNull();
        idFlag.Name.ShouldBe("id");
        idFlag.DataTypeName.ShouldBe("token");
        idFlag.FormalName.ShouldBe("Identifier");
        idFlag.Description.ShouldNotBeNull();
        idFlag.ContainingModule.ShouldBe(module);
    }

    [Fact]
    public void Load_SimpleModule_ShouldParseFieldDefinitions()
    {
        // Arrange
        var loader = new ModuleLoader();
        var path = GetTestDataPath("simple-module.xml");

        // Act
        var module = loader.Load(path);

        // Assert
        var fields = module.FieldDefinitions.ToList();
        fields.Count.ShouldBe(2);

        var descField = module.GetFieldDefinition("description");
        descField.ShouldNotBeNull();
        descField.Name.ShouldBe("description");
        descField.DataTypeName.ShouldBe("markup-multiline");
        descField.FlagInstances.Count.ShouldBe(1);
        descField.FlagInstances[0].Ref.ShouldBe("id");
        descField.FlagInstances[0].IsRequired.ShouldBeTrue();
    }

    [Fact]
    public void Load_SimpleModule_ShouldParseAssemblyDefinitions()
    {
        // Arrange
        var loader = new ModuleLoader();
        var path = GetTestDataPath("simple-module.xml");

        // Act
        var module = loader.Load(path);

        // Assert
        var assemblies = module.AssemblyDefinitions.ToList();
        assemblies.Count.ShouldBe(1);

        var docAssembly = module.GetAssemblyDefinition("document");
        docAssembly.ShouldNotBeNull();
        docAssembly.Name.ShouldBe("document");
        docAssembly.RootName.ShouldBe("document");
        docAssembly.IsRoot.ShouldBeTrue();
        docAssembly.FlagInstances.Count.ShouldBe(2);
        docAssembly.Model.ShouldNotBeNull();
        docAssembly.Model!.Elements.Count.ShouldBe(2);
    }

    [Fact]
    public void Load_SimpleModule_ShouldResolveReferences()
    {
        // Arrange
        var loader = new ModuleLoader();
        var path = GetTestDataPath("simple-module.xml");

        // Act
        var module = loader.Load(path);

        // Assert
        var docAssembly = module.GetAssemblyDefinition("document");
        docAssembly.ShouldNotBeNull();

        // Check that flag instances are resolved
        var idFlagInstance = docAssembly.FlagInstances.First(f => f.Ref == "id");
        idFlagInstance.ResolvedDefinition.ShouldNotBeNull();
        idFlagInstance.ResolvedDefinition!.Name.ShouldBe("id");

        // Check that model instances are resolved
        var titleInstance = docAssembly.Model!.Elements.OfType<FieldInstance>().First(f => f.Ref == "title");
        titleInstance.ResolvedDefinition.ShouldNotBeNull();
        titleInstance.ResolvedDefinition!.Name.ShouldBe("title");
    }

    [Fact]
    public void Load_SimpleModule_ShouldParseOccurrences()
    {
        // Arrange
        var loader = new ModuleLoader();
        var path = GetTestDataPath("simple-module.xml");

        // Act
        var module = loader.Load(path);
        var docAssembly = module.GetAssemblyDefinition("document");

        // Assert
        var titleInstance = docAssembly!.Model!.Elements.OfType<FieldInstance>().First(f => f.Ref == "title");
        titleInstance.MinOccurs.ShouldBe(1);
        titleInstance.MaxOccurs.ShouldBe(1);

        var descInstance = docAssembly!.Model!.Elements.OfType<FieldInstance>().First(f => f.Ref == "description");
        descInstance.MinOccurs.ShouldBe(0);
        descInstance.MaxOccurs.ShouldBeNull(); // unbounded
        descInstance.GroupAs.ShouldNotBeNull();
        descInstance.GroupAs!.Name.ShouldBe("descriptions");
    }

    [Fact]
    public void Load_ModuleWithImports_ShouldLoadImportedModules()
    {
        // Arrange
        var loader = new ModuleLoader();
        var path = GetTestDataPath("module-with-imports/main.xml");

        // Act
        var module = loader.Load(path);

        // Assert
        module.ImportedModules.Count.ShouldBe(1);
        var imported = module.ImportedModules[0];
        imported.ShortName.ShouldBe("imported");
    }

    [Fact]
    public void Load_ModuleWithImports_ShouldResolveImportedDefinitions()
    {
        // Arrange
        var loader = new ModuleLoader();
        var path = GetTestDataPath("module-with-imports/main.xml");

        // Act
        var module = loader.Load(path);

        // Assert
        // Should be able to resolve imported flag
        var sharedId = module.GetFlagDefinition("shared-id");
        sharedId.ShouldNotBeNull();
        sharedId.FormalName.ShouldBe("Shared Identifier");

        // Should be able to resolve imported field
        var sharedField = module.GetFieldDefinition("shared-field");
        sharedField.ShouldNotBeNull();

        // Local-only flag should NOT be visible
        var localOnly = module.GetFlagDefinition("local-only");
        localOnly.ShouldBeNull();
    }

    [Fact]
    public void Load_ModuleWithImports_ShouldResolveReferencesToImportedDefinitions()
    {
        // Arrange
        var loader = new ModuleLoader();
        var path = GetTestDataPath("module-with-imports/main.xml");

        // Act
        var module = loader.Load(path);

        // Assert
        var rootAssembly = module.GetAssemblyDefinition("root");
        rootAssembly.ShouldNotBeNull();

        // Flag instance should be resolved to imported definition
        var sharedIdInstance = rootAssembly.FlagInstances.First(f => f.Ref == "shared-id");
        sharedIdInstance.ResolvedDefinition.ShouldNotBeNull();
        sharedIdInstance.ResolvedDefinition!.ContainingModule.ShortName.ShouldBe("imported");

        // Field instance should be resolved to imported definition
        var sharedFieldInstance = rootAssembly.Model!.Elements.OfType<FieldInstance>().First();
        sharedFieldInstance.ResolvedDefinition.ShouldNotBeNull();
        sharedFieldInstance.ResolvedDefinition!.ContainingModule.ShortName.ShouldBe("imported");
    }

    [Fact]
    public void Load_SameModuleTwice_ShouldReturnCachedInstance()
    {
        // Arrange
        var loader = new ModuleLoader();
        var path = GetTestDataPath("simple-module.xml");

        // Act
        var module1 = loader.Load(path);
        var module2 = loader.Load(path);

        // Assert
        module1.ShouldBeSameAs(module2);
    }

    [Fact]
    public void Load_RootAssemblyDefinitions_ShouldReturnOnlyRoots()
    {
        // Arrange
        var loader = new ModuleLoader();
        var path = GetTestDataPath("simple-module.xml");

        // Act
        var module = loader.Load(path);

        // Assert
        var roots = module.RootAssemblyDefinitions.ToList();
        roots.Count.ShouldBe(1);
        roots[0].Name.ShouldBe("document");
    }

    [Fact]
    public void Load_ExportedDefinitions_ShouldOnlyIncludeGlobalScope()
    {
        // Arrange
        var loader = new ModuleLoader();
        var path = GetTestDataPath("module-with-imports/imported.xml");

        // Act
        var module = loader.Load(path);

        // Assert
        // Should include shared-id and shared-field, but not local-only
        var exportedFlags = module.ExportedFlagDefinitions.ToList();
        var exportedFields = module.ExportedFieldDefinitions.ToList();
        (exportedFlags.Count + exportedFields.Count).ShouldBe(2);
        exportedFlags.ShouldNotContain(d => d.Name == "local-only");
    }
}
