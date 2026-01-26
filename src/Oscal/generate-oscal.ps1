#!/usr/bin/env pwsh

$OutputDir = "V1_2_0/Generated"

if (Test-Path $OutputDir) {
    Remove-Item $OutputDir -Recurse -Force
}

dotnet run --project ../../src/Metaschema.Tool/Metaschema.Tool.csproj -- `
    generate-code ../../reference/oscal/v1.2.0/oscal_complete_metaschema.xml `
    --namespace Oscal.V1_2_0 `
    --output $OutputDir
