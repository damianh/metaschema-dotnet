// Licensed under the MIT License.

using Metaschema.Core.Loading;
using Oscal.V1_2_0;
using Shouldly;
using Xunit;

namespace Oscal;

/// <summary>
/// Smoke tests for OSCAL library code generation.
/// These tests verify that the OSCAL types were generated correctly.
/// </summary>
public class OscalSmokeTests
{
    [Fact]
    public void OscalMetaschema_HashField_ShouldHaveAlgorithmFlag()
    {
        // Arrange - Load the OSCAL metadata metaschema
        var loader = new ModuleLoader();
        var metaschemaPath = Path.Combine(
            AppContext.BaseDirectory, 
            "..", "..", "..", "..", "..", 
            "samples", "oscal-metaschema-v1.2.0", "oscal_metadata_metaschema.xml");

        // Act
        var module = loader.Load(metaschemaPath);

        // Assert
        var hashField = module.GetFieldDefinition("hash");
        hashField.ShouldNotBeNull("Hash field should be defined in OSCAL metadata metaschema");
        
        hashField.FlagInstances.Count.ShouldBeGreaterThan(0, "Hash field should have flag instances");
        
        var algorithmFlag = hashField.FlagInstances.FirstOrDefault(f => f.EffectiveName == "algorithm");
        algorithmFlag.ShouldNotBeNull("Hash field should have 'algorithm' flag");
        algorithmFlag.IsRequired.ShouldBeTrue("Algorithm flag should be required");
        
        // Verify the inline flag definition is attached
        algorithmFlag.ResolvedDefinition.ShouldNotBeNull("Inline flag should have definition attached");
        algorithmFlag.ResolvedDefinition!.DataTypeName.ShouldBe("string");
    }

    [Fact]
    public void CatalogType_ShouldBeGenerated()
    {
        // Act - verify type exists and can be instantiated
        var catalog = typeof(Catalog);

        // Assert
        catalog.ShouldNotBeNull();
        catalog.IsSealed.ShouldBeTrue("Generated types should be sealed records");
        catalog.GetProperty("Metadata").ShouldNotBeNull();
        catalog.GetProperty("Groups").ShouldNotBeNull();
        catalog.GetProperty("Controls").ShouldNotBeNull();
    }

    [Fact]
    public void ProfileType_ShouldBeGenerated()
    {
        // Act - verify type exists
        var profile = typeof(Profile);

        // Assert
        profile.ShouldNotBeNull();
        profile.IsSealed.ShouldBeTrue("Generated types should be sealed records");
        profile.GetProperty("Metadata").ShouldNotBeNull();
        profile.GetProperty("Imports").ShouldNotBeNull();
    }

    [Fact]
    public void SystemSecurityPlanType_ShouldBeGenerated()
    {
        // Act - verify type exists
        var ssp = typeof(SystemSecurityPlan);

        // Assert
        ssp.ShouldNotBeNull();
        ssp.IsSealed.ShouldBeTrue("Generated types should be sealed records");
        ssp.GetProperty("Metadata").ShouldNotBeNull();
        ssp.GetProperty("SystemCharacteristics").ShouldNotBeNull();
    }

    [Fact]
    public void ComponentDefinitionType_ShouldBeGenerated()
    {
        // Act - verify type exists
        var componentDef = typeof(ComponentDefinition);

        // Assert
        componentDef.ShouldNotBeNull();
        componentDef.IsSealed.ShouldBeTrue("Generated types should be sealed records");
        componentDef.GetProperty("Metadata").ShouldNotBeNull();
        componentDef.GetProperty("Components").ShouldNotBeNull();
    }

    [Fact]
    public void AssessmentPlanType_ShouldBeGenerated()
    {
        // Act - verify type exists
        var assessmentPlan = typeof(AssessmentPlan);

        // Assert
        assessmentPlan.ShouldNotBeNull();
        assessmentPlan.IsSealed.ShouldBeTrue("Generated types should be sealed records");
        assessmentPlan.GetProperty("Metadata").ShouldNotBeNull();
    }

    [Fact]
    public void AssessmentResultsType_ShouldBeGenerated()
    {
        // Act - verify type exists
        var assessmentResults = typeof(AssessmentResults);

        // Assert
        assessmentResults.ShouldNotBeNull();
        assessmentResults.IsSealed.ShouldBeTrue("Generated types should be sealed records");
        assessmentResults.GetProperty("Metadata").ShouldNotBeNull();
        assessmentResults.GetProperty("Results").ShouldNotBeNull();
    }

    [Fact]
    public void PlanOfActionAndMilestonesType_ShouldBeGenerated()
    {
        // Act - verify type exists
        var poam = typeof(PlanOfActionAndMilestones);

        // Assert
        poam.ShouldNotBeNull();
        poam.IsSealed.ShouldBeTrue("Generated types should be sealed records");
        poam.GetProperty("Metadata").ShouldNotBeNull();
    }

    [Fact]
    public void MappingCollectionType_ShouldBeGenerated()
    {
        // Act - verify type exists
        var mappingCollection = typeof(MappingCollection);

        // Assert
        mappingCollection.ShouldNotBeNull();
        mappingCollection.IsSealed.ShouldBeTrue("Generated types should be sealed records");
        mappingCollection.GetProperty("Metadata").ShouldNotBeNull();
    }

    [Fact]
    public void V1_2_0JsonContext_ShouldBeGenerated()
    {
        // Act - verify JSON context exists
        var jsonContext = typeof(V1_2_0JsonContext);

        // Assert
        jsonContext.ShouldNotBeNull();
        V1_2_0JsonContext.Default.ShouldNotBeNull("JSON context should have Default singleton");
        V1_2_0JsonContext.Default.Options.ShouldNotBeNull("JSON context should provide options");
    }

    [Fact]
    public void MetadataType_ShouldHaveRequiredProperties()
    {
        // Act - verify Metadata type structure
        var metadata = typeof(Metadata);

        // Assert
        metadata.ShouldNotBeNull();
        metadata.IsSealed.ShouldBeTrue("Generated types should be sealed records");
        
        var lastModified = metadata.GetProperty("LastModified");
        lastModified.ShouldNotBeNull();
        
        var version = metadata.GetProperty("Version");
        version.ShouldNotBeNull();
        
        var oscalVersion = metadata.GetProperty("OscalVersion");
        oscalVersion.ShouldNotBeNull();
    }
}
