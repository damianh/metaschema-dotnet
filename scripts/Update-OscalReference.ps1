#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Fetches OSCAL metaschema files from the NIST OSCAL repository.

.DESCRIPTION
    This script clones the usnistgov/OSCAL repository at a specific version tag,
    copies the metaschema files to reference/oscal/{version}/, and updates the
    versions.json manifest.

.PARAMETER Version
    The OSCAL version to fetch (e.g., "1.2.0"). This will fetch from tag "v{Version}".

.EXAMPLE
    ./Update-OscalReference.ps1 -Version 1.2.0
    
    Fetches OSCAL v1.2.0 metaschema files to reference/oscal/v1.2.0/

.EXAMPLE
    ./Update-OscalReference.ps1 -Version 1.1.2
    
    Fetches OSCAL v1.1.2 metaschema files to reference/oscal/v1.1.2/
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$Version
)

$ErrorActionPreference = "Stop"

# Validate version format (basic semver check)
if ($Version -notmatch '^\d+\.\d+\.\d+$') {
    Write-Error "Version must be in format X.Y.Z (e.g., 1.2.0)"
    exit 1
}

$tag = "v$Version"
$repoUrl = "https://github.com/usnistgov/OSCAL.git"
$scriptDir = $PSScriptRoot
$rootDir = Split-Path $scriptDir -Parent
$referenceDir = Join-Path $rootDir "reference" "oscal"
$versionDir = Join-Path $referenceDir "v$Version"
$manifestPath = Join-Path $referenceDir "versions.json"
$tempDir = Join-Path ([System.IO.Path]::GetTempPath()) "oscal-fetch-$([Guid]::NewGuid())"

Write-Host "=== OSCAL Metaschema Reference Update ===" -ForegroundColor Cyan
Write-Host "Version:        $Version" -ForegroundColor White
Write-Host "Tag:            $tag" -ForegroundColor White
Write-Host "Destination:    $versionDir" -ForegroundColor White
Write-Host ""

# Check if version already exists
if (Test-Path $versionDir) {
    Write-Warning "Version $Version already exists at $versionDir"
    $response = Read-Host "Overwrite? (y/n)"
    if ($response -ne 'y') {
        Write-Host "Aborted." -ForegroundColor Yellow
        exit 0
    }
    Write-Host "Removing existing version..." -ForegroundColor Yellow
    Remove-Item $versionDir -Recurse -Force
}

# Create reference/oscal directory if it doesn't exist
if (-not (Test-Path $referenceDir)) {
    Write-Host "Creating reference/oscal directory..." -ForegroundColor Green
    New-Item -ItemType Directory -Path $referenceDir -Force | Out-Null
}

try {
    # Create temp directory
    Write-Host "Creating temporary directory..." -ForegroundColor Gray
    New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
    
    # Shallow clone the OSCAL repo at the specific tag
    Write-Host "Cloning OSCAL repository (tag: $tag)..." -ForegroundColor Green
    $cloneResult = git clone --depth 1 --branch $tag $repoUrl $tempDir 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Git clone failed. Error: $cloneResult"
        exit 1
    }
    
    # Check if src/metaschema exists
    $metaschemaSource = Join-Path $tempDir "src" "metaschema"
    if (-not (Test-Path $metaschemaSource)) {
        Write-Error "Metaschema directory not found at $metaschemaSource"
        exit 1
    }
    
    # Copy metaschema files to destination
    Write-Host "Copying metaschema files to $versionDir..." -ForegroundColor Green
    Copy-Item -Path $metaschemaSource -Destination $versionDir -Recurse -Force
    
    # Count files copied
    $fileCount = (Get-ChildItem -Path $versionDir -Recurse -File).Count
    Write-Host "✓ Copied $fileCount files" -ForegroundColor Green
    
    # Update versions.json manifest
    Write-Host "Updating versions.json manifest..." -ForegroundColor Green
    
    $manifest = @{
        versions = @()
    }
    
    # Load existing manifest if it exists
    if (Test-Path $manifestPath) {
        $existingManifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
        $manifest.versions = @($existingManifest.versions | Where-Object { $_.version -ne $Version })
    }
    
    # Add new version entry
    $versionEntry = @{
        version = $Version
        tag = $tag
        fetchedAt = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    }
    
    $manifest.versions += $versionEntry
    $manifest.versions = $manifest.versions | Sort-Object -Property version -Descending
    
    # Write manifest
    $manifest | ConvertTo-Json -Depth 10 | Set-Content $manifestPath -Encoding UTF8
    
    Write-Host ""
    Write-Host "=== Success ===" -ForegroundColor Green
    Write-Host "OSCAL v$Version metaschema files fetched successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Location:    $versionDir" -ForegroundColor White
    Write-Host "Files:       $fileCount" -ForegroundColor White
    Write-Host "Manifest:    $manifestPath" -ForegroundColor White
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "  1. Review the fetched files" -ForegroundColor White
    Write-Host "  2. Update code references to use reference/oscal/v$Version/" -ForegroundColor White
    Write-Host "  3. Test code generation and validation" -ForegroundColor White
    Write-Host "  4. Commit the changes" -ForegroundColor White
    
} catch {
    Write-Error "An error occurred: $_"
    exit 1
} finally {
    # Clean up temp directory
    if (Test-Path $tempDir) {
        Write-Host "Cleaning up temporary files..." -ForegroundColor Gray
        Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue
    }
}
