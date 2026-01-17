// Licensed under the MIT License.

using Metaschema.Core.Model;
using Shouldly;
using Xunit;

namespace Metaschema.Core.Tests.Model;

public class MetaschemaModuleTests
{
    private static MetaschemaModule CreateTestModule() => new()
    {
        Name = "Test Module",
        ShortName = "test",
        Version = "1.0.0",
        XmlNamespace = new Uri("http://example.com/ns/test"),
        JsonBaseUri = new Uri("http://example.com/schema/test"),
        Location = new Uri("file:///test.xml")
    };

    [Fact]
    public void GetFlagDefinition_LocalDefinition_ShouldReturnLocal()
    {
        // Arrange
        var module = CreateTestModule();
        var flag = new FlagDefinition
        {
            Name = "my-flag",
            ContainingModule = module
        };
        module.AddFlagDefinition(flag);

        // Act
        var result = module.GetFlagDefinition("my-flag");

        // Assert
        result.ShouldBe(flag);
    }

    [Fact]
    public void GetFlagDefinition_NotFound_ShouldReturnNull()
    {
        // Arrange
        var module = CreateTestModule();

        // Act
        var result = module.GetFlagDefinition("nonexistent");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetFlagDefinition_LocalShadowsImport_ShouldReturnLocal()
    {
        // Arrange
        var importedModule = CreateTestModule();
        var importedFlag = new FlagDefinition
        {
            Name = "shared-flag",
            FormalName = "Imported",
            ContainingModule = importedModule,
            Scope = Scope.Global
        };
        importedModule.AddFlagDefinition(importedFlag);

        var mainModule = CreateTestModule();
        var localFlag = new FlagDefinition
        {
            Name = "shared-flag",
            FormalName = "Local",
            ContainingModule = mainModule
        };
        mainModule.AddFlagDefinition(localFlag);
        mainModule.AddImportedModule(importedModule);

        // Act
        var result = mainModule.GetFlagDefinition("shared-flag");

        // Assert
        result.ShouldBe(localFlag);
        result!.FormalName.ShouldBe("Local");
    }

    [Fact]
    public void GetFlagDefinition_LaterImportShadowsEarlier_ShouldReturnLater()
    {
        // Arrange
        var firstImport = CreateTestModule();
        var firstFlag = new FlagDefinition
        {
            Name = "shared-flag",
            FormalName = "First",
            ContainingModule = firstImport,
            Scope = Scope.Global
        };
        firstImport.AddFlagDefinition(firstFlag);

        var secondImport = CreateTestModule();
        var secondFlag = new FlagDefinition
        {
            Name = "shared-flag",
            FormalName = "Second",
            ContainingModule = secondImport,
            Scope = Scope.Global
        };
        secondImport.AddFlagDefinition(secondFlag);

        var mainModule = CreateTestModule();
        mainModule.AddImportedModule(firstImport);
        mainModule.AddImportedModule(secondImport);

        // Act
        var result = mainModule.GetFlagDefinition("shared-flag");

        // Assert
        result.ShouldBe(secondFlag);
        result!.FormalName.ShouldBe("Second");
    }

    [Fact]
    public void ExportedDefinitions_ShouldOnlyIncludeGlobalScope()
    {
        // Arrange
        var module = CreateTestModule();
        var globalFlag = new FlagDefinition
        {
            Name = "global-flag",
            ContainingModule = module,
            Scope = Scope.Global
        };
        var localFlag = new FlagDefinition
        {
            Name = "local-flag",
            ContainingModule = module,
            Scope = Scope.Local
        };
        module.AddFlagDefinition(globalFlag);
        module.AddFlagDefinition(localFlag);

        // Act
        var exported = module.ExportedFlagDefinitions.ToList();

        // Assert
        exported.Count.ShouldBe(1);
        exported[0].Name.ShouldBe("global-flag");
    }

    [Fact]
    public void RootAssemblyDefinitions_ShouldOnlyIncludeRoots()
    {
        // Arrange
        var module = CreateTestModule();
        var rootAssembly = new AssemblyDefinition
        {
            Name = "root-assembly",
            RootName = "root",
            ContainingModule = module
        };
        var nonRootAssembly = new AssemblyDefinition
        {
            Name = "non-root-assembly",
            RootName = null,
            ContainingModule = module
        };
        module.AddAssemblyDefinition(rootAssembly);
        module.AddAssemblyDefinition(nonRootAssembly);

        // Act
        var roots = module.RootAssemblyDefinitions.ToList();

        // Assert
        roots.Count.ShouldBe(1);
        roots[0].Name.ShouldBe("root-assembly");
    }

    [Fact]
    public void EffectiveName_WithUseName_ShouldReturnUseName()
    {
        // Arrange
        var module = CreateTestModule();
        var flag = new FlagDefinition
        {
            Name = "original-name",
            UseName = "effective-name",
            ContainingModule = module
        };

        // Act & Assert
        flag.EffectiveName.ShouldBe("effective-name");
    }

    [Fact]
    public void EffectiveName_WithoutUseName_ShouldReturnName()
    {
        // Arrange
        var module = CreateTestModule();
        var flag = new FlagDefinition
        {
            Name = "original-name",
            UseName = null,
            ContainingModule = module
        };

        // Act & Assert
        flag.EffectiveName.ShouldBe("original-name");
    }
}
