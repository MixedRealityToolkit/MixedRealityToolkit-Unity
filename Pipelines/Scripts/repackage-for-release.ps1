# Copyright (c) Mixed Reality Toolkit Contributors
# Licensed under the BSD 3-Clause

<#
.SYNOPSIS
    Repackages release candidate tgz to  the Mixed Reality Toolkit Unity Package Manager (UPM) packages.
.DESCRIPTION
    Builds UPM packages for the Mixed Reality Toolkit.
.PARAMETER PackageRoot
    The root folder of the project.
.PARAMETER OutputDirectory
    Where should we place the output? Defaults to ".\artifacts"
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$PackageRoot,
    [string]$OutputDirectory = "./artifacts/upm/release" 
)

$PackageRoot = Resolve-Path -Path $PackageRoot

if (-not (Test-Path $OutputDirectory -PathType Container)) {
    New-Item $OutputDirectory -ItemType Directory | Out-Null
}

$OutputDirectory = Resolve-Path -Path $OutputDirectory

Write-Host ""
Write-Host -ForegroundColor Blue "======================================="  
Write-Host -ForegroundColor Blue "Repackaging All Packages For Release"
Write-Host -ForegroundColor Blue "======================================="
Write-Host "OutputDirectory: $OutputDirectory"
Write-Host "Release packages: $releasePkgs"

try {           
    $repackTempDirectory = Join-Path $OutputDirectory "tmp"
    Write-Host "Temp Directory: $repackTempDirectory"

    if (-not (Test-Path $repackTempDirectory -PathType Container)) {
        New-Item $repackTempDirectory -ItemType Directory | Out-Null
    }
    
    Get-ChildItem -Path (Join-Path $PackageRoot "*.tgz") | ForEach-Object {
        New-Item -ItemType Directory -Force (Join-Path $repackTempDirectory $_.BaseName)
        tar -xzf $_ -C (Join-Path $repackTempDirectory $_.BaseName)
    }

    $packageSearchPath = "$repackTempDirectory\*\package\package.json"
    Write-Host "PackageSearchPath: $packageSearchPath"

    # Update package versions to their release versions
    Get-ChildItem -Path $packageSearchPath | ForEach-Object {
        $packageName = Select-String -Pattern "org\.mixedrealitytoolkit\.\w+(\.\w+)*|com\.microsoft\.mrtk\.\w+(\.\w+)*" -Path $_.FullName | Select-Object -First 1

        if (-not $packageName) {
            return # this is not an MRTK package, so skip
        }

        $packageName = $packageName.Matches[0].Value
        $packagePath = $_.Directory

        Write-Host ""
        Write-Host -ForegroundColor Green "======================================="  
        Write-Host -ForegroundColor Green "Updating package to release package"
        Write-Host -ForegroundColor Green "======================================="

        $inlineVersion = Select-String '^.*"version":\s*"(?<sem>[0-9]\.[0-9]\.[0-9])(-(?<label>[a-zA-Z]+)(\.(?<experiment>[a-zA-Z]+\.\d+))?(\.(?<revision>\d(\.\d+)*))?)?' -InputObject (Get-Content -Path $_)
        $version = $inlineVersion.Matches[0].Groups['sem'].Value
        $releaseLabel = $inlineVersion.Matches[0].Groups['label'].Value
        $experimentLabel = $inlineVersion.Matches[0].Groups['experiment'].Value
        $revision = $inlineVersion.Matches[0].Groups['revision'].Value

        Write-Host "Package name: $packageName" 
        Write-Host "Old version: $version" 
        Write-Host "Old release label: $releaseLabel" 
        Write-Host "Old experiment label: $experimentLabel" 
        Write-Host "Old revision: $revision" 
        
        # Update package versions
        . $PSScriptRoot\update-versions.ps1 -PackagesRoot $packagePath -ExperimentLabel $experimentLabel -ReleasePackages $ReleasePackages
    }

    # Repackage the package directories
    Get-ChildItem -Path $packageSearchPath | ForEach-Object {
        $currentPackageName = Select-String -Pattern "org\.mixedrealitytoolkit\.\w+(\.\w+)*|com\.microsoft\.mrtk\.\w+(\.\w+)*" -Path $_.FullName | Select-Object -First 1

        if (-not $currentPackageName) {
            return # this is not an MRTK package, so skip
        }

        Write-Host ""
        Write-Host -ForegroundColor Green "======================================="  
        Write-Host -ForegroundColor Green "Packing Release Package"
        Write-Host -ForegroundColor Green "======================================="
        Write-Host "Package name: $currentPackageName" 

        $currentPackageName = $currentPackageName.Matches[0].Value
        $packageFriendlyName = (Select-String -Pattern "`"displayName`": `"(.+)`"" -Path $_ | Select-Object -First 1).Matches.Groups[1].Value

        $packagePath = $_.Directory

        Write-Output "Packing $packageFriendlyName to $OutputDirectory"
        npm pack $packagePath -pack-destination $OutputDirectory
    }

    Write-Host ""
    Write-Host -ForegroundColor Blue "======================================="  
    Write-Host -ForegroundColor Blue "Successfully Packed Release Packages"
    Write-Host -ForegroundColor Blue "=======================================" 
    Write-Host ""
}
finally {
    Remove-Item -Force -Recurse $repackTempDirectory
}
